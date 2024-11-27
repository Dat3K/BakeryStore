$(document).ready(function() {
    // Handle Add to Cart button clicks
    $('.addtocart-btn').click(function(e) {
        e.preventDefault();
        
        // Check if user is authenticated using data attribute
        var isAuthenticated = $(this).data('authenticated') === true;
        
        if (!isAuthenticated) {
            // Show login modal if user is not authenticated
            $('#loginModal').modal('show');
            return;
        }
        
        // If authenticated, proceed with adding to cart
        // Add your cart logic here
    });
});
