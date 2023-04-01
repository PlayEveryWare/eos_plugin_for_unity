PlayEveryWare EOS Performance Stress Test

This demo is intended to show the plugin and overlay works under a simulated load

How to use:
There are 3 tests:
	CPU - a multi-core CPU load
		The target utilization slider selects the desired CPU utilization for the tests
		The core count slider selects the number of cores to run the test on
		The Detect Core Count toggle will automatically detect the total number of threads available to the computer, and use that for the test
		
	GPU - a simulated load for the GPU
		Due to the wide variety of harrdware the GPU test is a heavy load scene that should reach 80% or more utilization on any GPU
		
	Memory - a memory allocation tool
		This test allocates the desired ammount of memory. 
		Note: The memory allocated is freed once the test is complete, however Unity still holds ont to it,
			and it is not made available to the rest of the system until the application has been closed