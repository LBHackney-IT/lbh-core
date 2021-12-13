# Hackney.Core.Authorization NuGet Package
Common library providing functionality to validates that provided in request JWT token contains correct Google Group. Used in LBH Web API services.

## Usage

To use this middleware you need to add the next line in your `Configure(...)` methon in `Startup.cs` file:
    
	    app.UseGoogleGroupAuthorization();
	
This middleware will use `REQUIRED_GOOGL_GROUPS` environment valiable to get required Google groups list