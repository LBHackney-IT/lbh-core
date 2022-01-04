# Hackney.Core.Authorization NuGet Package
Common library providing functionality to validates that provided in request JWT token contains correct Google Group. Used in LBH Web API services.

## Usage

To use this middleware you need to add the next line in your `Configure(...)` methon in `Startup.cs` file:
    
	    app.UseGoogleGroupAuthorization();
	
This middleware will use the next environment valiables:  
*  `REQUIRED_GOOGLE_GROUPS` to get required Google groups list
*  `URLS_TO_SKIP_AUTH` to get URLs to skip authorization. For example, we need to skip authorization for swagger 

