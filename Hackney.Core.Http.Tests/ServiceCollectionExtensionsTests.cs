﻿using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Hackney.Core.Http.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHttpContextWrapperTestNullServicesThrows()
        {
            IServiceCollection services = null;
            Action act = () => ServiceCollectionExtensions.AddHttpContextWrapper(services);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AddHttpContextWrapperTestAddsRequiredTypes()
        {
            var services = new ServiceCollection();
            services.AddHttpContextWrapper();
            services.IsServiceRegistered<IHttpContextWrapper, HttpContextWrapper>().Should().BeTrue();
        }
    }
}