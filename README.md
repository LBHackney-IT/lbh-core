# Set Up NuGet Package
At Hackney, we have created the NuGet Package to prevent the duplication of common code when implementing our APIs, Hence this NuGet Package will store the common code that can then be added to the relevant projects. 

## CircleCI Pipeline 
For NuGet Push command to work, you will need to change the version in the .csproj file according to the type of change you are making:

    A specific version number is in the form Major.Minor.Patch[-Suffix], where the components have the following meanings:

    Major: Breaking changes
    Minor: New features, but backward compatible
    Patch: Backwards compatible bug fixes only
    Suffix (optional): a hyphen followed by a string denoting a pre-release version
