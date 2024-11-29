$(document).ready(function() {
    // Function to update cart count
    function updateCartCount() {
        $.ajax({
            url: '/Store/Cart/GetCartCount',
            type: 'GET',
            success: function(response) {
                if (response.success) {
                    $('.cart-btn a span').text(response.count);
                }
            }
        });
    }

    // Update cart count on page load
    updateCartCount();

    // Handle Add to Cart button clicks
    function addToCart(productId, quantity) {
        $.ajax({
            url: '/Store/Cart/AddToCart',
            type: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function(response) {
                if (response.success) {
                    updateCartCount();
                    Swal.fire({
                        title: 'Added to Cart',
                        icon: 'success',
                        showConfirmButton: false,
                        timer: 1500
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.message || 'Failed to add item to cart',
                        icon: 'error'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Error',
                    text: 'Failed to add item to cart',
                    icon: 'error'
                });
            }
        });
    }

    // Make addToCart function globally accessible
    window.addToCart = addToCart;

    // Debounce function to limit rate of function calls
    function debounce(func, wait) {
        let timeout;
        return function(...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    // Advanced request queue with state tracking
    class RequestQueue {
        constructor() {
            this.queue = new Map(); // Map of itemId to latest quantity
            this.processing = false;
            this.processDebounced = debounce(this.processQueue.bind(this), 500);
        }

        // Add or update request in queue
        add(itemId, quantity) {
            this.queue.set(itemId, quantity);
            this.processDebounced();
        }

        // Process all queued requests
        async processQueue() {
            if (this.processing || this.queue.size === 0) return;
            
            this.processing = true;
            const currentQueue = new Map(this.queue);
            this.queue.clear();

            try {
                // Process all queued items in parallel
                const updates = Array.from(currentQueue).map(([itemId, quantity]) => 
                    this.updateQuantity(itemId, quantity)
                );
                
                await Promise.all(updates);
            } catch (error) {
                console.error('Error processing queue:', error);
            } finally {
                this.processing = false;
                // If new items were queued during processing, process them
                if (this.queue.size > 0) {
                    this.processDebounced();
                }
            }
        }

        // Update single item quantity
        async updateQuantity(itemId, quantity) {
            try {
                const response = await $.ajax({
                    url: '/Store/Cart/UpdateQuantity',
                    type: 'POST',
                    data: { itemId, quantity }
                });

                if (!response.success) {
                    throw new Error(response.message || 'Failed to update quantity');
                }
                
                // Update cart count only after successful update
                updateCartCount();
                
            } catch (error) {
                Swal.fire({
                    title: 'Error',
                    text: error.message || 'Failed to update quantity',
                    icon: 'error'
                });
                // Revert UI to match server state
                const serverQuantity = await this.fetchCurrentQuantity(itemId);
                updateUIPrice(itemId, serverQuantity);
            }
        }

        // Fetch current quantity from server
        async fetchCurrentQuantity(itemId) {
            try {
                const response = await $.ajax({
                    url: '/Store/Cart/GetItemQuantity',
                    type: 'GET',
                    data: { itemId }
                });
                return response.quantity || 1;
            } catch (error) {
                console.error('Error fetching quantity:', error);
                return 1;
            }
        }
    }

    // Create single instance of RequestQueue
    const requestQueue = new RequestQueue();

    // Function to update UI prices
    function updateUIPrice(itemId, quantity) {
        const row = $(`input[data-item-id="${itemId}"]`).closest('tr');
        const price = parseFloat(row.find('.price').text().replace('$', ''));
        const newSubtotal = (price * quantity).toFixed(2);
        row.find('.sub-total').text('$' + newSubtotal);

        // Update cart totals
        let cartTotal = 0;
        $('.sub-total').each(function() {
            cartTotal += parseFloat($(this).text().replace('$', ''));
        });
        
        cartTotal = cartTotal.toFixed(2);
        $('.total-cart-box .list li:first-child span').text('$' + cartTotal);
        $('.total-cart-box .list li:last-child span').text('$' + cartTotal);
    }

    // Function to handle quantity updates
    function handleQuantityUpdate(itemId, quantity) {
        // Update UI immediately for better UX
        updateUIPrice(itemId, quantity);
        // Queue the server update
        requestQueue.add(itemId, quantity);
    }

    // Handle quantity input changes with validation
    $(document).on('change', '.quantity-input .quantity-field', function() {
        const input = $(this);
        let quantity = parseInt(input.val()) || 1;
        const itemId = input.data('item-id');
        const maxQuantity = 100;
        
        // Validate and adjust quantity
        if (quantity < 1) quantity = 1;
        if (quantity > maxQuantity) quantity = maxQuantity;
        
        // Update input value if it was adjusted
        if (quantity !== parseInt(input.val())) {
            input.val(quantity);
        }
        
        handleQuantityUpdate(itemId, quantity);
    });

    // Handle increase button with validation
    $(document).on('click', '.quantity-input .increase-btn', function() {
        const input = $(this).siblings('.quantity-field');
        const currentValue = parseInt(input.val()) || 1;
        const maxQuantity = 100;
        
        if (currentValue < maxQuantity) {
            input.val(currentValue + 1).trigger('change');
        } else {
            Swal.fire({
                title: 'Maximum Quantity',
                text: 'You cannot add more than 100 items',
                icon: 'warning',
                timer: 2000,
                showConfirmButton: false
            });
        }
    });

    // Handle decrease button with validation
    $(document).on('click', '.quantity-input .decrease-btn', function() {
        const input = $(this).siblings('.quantity-field');
        const currentValue = parseInt(input.val()) || 1;
        
        if (currentValue > 1) {
            input.val(currentValue - 1).trigger('change');
        }
    });

    // Handle remove item from cart
    $(document).on('click', '.remove-cart-item', function(e) {
        e.preventDefault();
        const itemId = $(this).data('item-id');
        
        Swal.fire({
            title: 'Are you sure?',
            text: "Remove this item from cart?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, remove it!',
            cancelButtonText: 'No, cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Store/Cart/RemoveItem',
                    type: 'POST',
                    data: { itemId: itemId },
                    success: function(response) {
                        if (response.success) {
                            // Remove the row from the table
                            $(`[data-item-id="${itemId}"]`).closest('tr').remove();
                            // Update totals
                            updateUIPrice(itemId, 0);
                            // Update cart count
                            updateCartCount();
                        } else {
                            Swal.fire({
                                title: 'Error',
                                text: response.message || 'Failed to remove item',
                                icon: 'error'
                            });
                        }
                    }
                });
            }
        });
    });
});
