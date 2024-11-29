$(document).ready(function() {
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

    // Handle quantity change in cart
    $('.quantity-spinner').on('change', function(e) {
        e.preventDefault();
        var itemId = $(this).data('item-id');
        var quantity = $(this).val();

        $.ajax({
            url: '/Store/Cart/UpdateQuantity',
            type: 'POST',
            data: {
                itemId: itemId,
                quantity: quantity
            },
            success: function(response) {
                if (response.success) {
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.message || 'Failed to update quantity',
                        icon: 'error'
                    });
                }
            }
        });
    });

    // Handle remove item from cart
    $('.remove-cart-item').on('click', function() {
        var itemId = $(this).data('item-id');
        
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
                    data: {
                        itemId: itemId
                    },
                    success: function(response) {
                        if (response.success) {
                            location.reload();
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
