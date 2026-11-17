using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Hackney.Core.Authorization.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Hackney.Core.Tests.Authorization.Internal;

public class ForbiddenAuthorizationResultHandlerTests
{
    private readonly ForbiddenAuthorizationResultHandler _classUnderTest;
    private readonly AuthorizationPolicy _policy;

    public ForbiddenAuthorizationResultHandlerTests()
    {
        _classUnderTest = new ForbiddenAuthorizationResultHandler();
        // The handler does not read the policy, however, HandleAsync requires one.
        _policy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
    }

    #region helper methods

    private Task Invoke(HttpContext context, PolicyAuthorizationResult result, Action? duringPipeline = null) =>
        _classUnderTest.HandleAsync(_ =>
        {
            duringPipeline?.Invoke();
            return Task.CompletedTask;
        }, context, _policy, result);

    #endregion

    #region allowed

    [Fact]
    public async Task HandleAsync_RunsTheRestOfThePipeline_WhenThePolicyAllowsTheRequest()
    {
        // arrange
        var context = new DefaultHttpContext();
        var pipelineRan = false;

        // act
        await Invoke(
            context,
            PolicyAuthorizationResult.Success(),
            () =>
            {
                pipelineRan = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
            });

        // assert
        pipelineRan.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    #endregion

    #region denied

    // Challenge is the failure for an anonymous caller. The built-in handler would answer it with 401.
    // Forbid is the failure for a caller ASP.NET already considers authenticated.
    // This handler writes 403 for both and does not run the rest of the pipeline.
    [Theory]
    [InlineData(Denial.Challenge)]
    [InlineData(Denial.Forbid)]
    public async Task HandleAsync_SetsForbiddenAndStopsThePipeline_WhenThePolicyDeniesTheRequest(Denial denial)
    {
        // arrange
        var context = new DefaultHttpContext();
        var pipelineRan = false;

        // act
        await Invoke(context, Denied(denial), () => pipelineRan = true);

        // assert
        pipelineRan.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    public enum Denial
    {
        Challenge,
        Forbid
    }

    private static PolicyAuthorizationResult Denied(Denial denial) => denial switch
    {
        Denial.Challenge => PolicyAuthorizationResult.Challenge(),
        Denial.Forbid => PolicyAuthorizationResult.Forbid(),
        _ => throw new ArgumentOutOfRangeException(nameof(denial))
    };

    #endregion
}
