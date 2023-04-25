# Ecom API notes
The Ecom API requires use of the overlay to work, but the EOS SDK doesn't limit grabbing the interface directly. 
This means it's possible to get the interface and try to use it even if it's not technically possible to use it.
Furthermore, this means on platforms that don't support the overlay, ECom API isn't usable.
