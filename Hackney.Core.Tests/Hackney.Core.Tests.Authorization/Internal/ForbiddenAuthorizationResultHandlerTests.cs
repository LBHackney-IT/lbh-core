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
        // The handler does not read the policy. HandleAsync requires one.
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

    [Fact]
    public async Task HandleAsync_SetsForbiddenAndStopsThePipeline_WhenTheResultIsAChallenge()
    {
        // arrange
        // Challenge is the failure for an anonymous caller. The built-in handler would answer it with 401.
        var context = new DefaultHttpContext();
        var pipelineRan = false;

        // act
        await Invoke(context, PolicyAuthorizationResult.Challenge(), () => pipelineRan = true);

        // assert
        pipelineRan.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task HandleAsync_SetsForbiddenAndStopsThePipeline_WhenTheResultIsAForbid()
    {
        // arrange
        // Forbid is the failure for a caller ASP.NET already considers authenticated.
        var context = new DefaultHttpContext();
        var pipelineRan = false;

        // act
        await Invoke(context, PolicyAuthorizationResult.Forbid(), () => pipelineRan = true);

        // assert
        pipelineRan.Should().BeFalse();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    #endregion
}
