using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.JWT;

internal static class TokenMapper
{
    // Separator aws cognito pre-token lambda uses to join Google group names with
    private const char CognitoTokenGoogleGroupsSeparator = ';';

    /// <summary>
    /// Map the presentation DTO to the legacy domain <see cref="Token"/>, normalising group values.
    /// </summary>
    /// <param name="presentation">The deserialised token presentation DTO.</param>
    /// <returns>The mapped domain <see cref="Token"/>.</returns>
    internal static Token MapStandardToken(TokenPresentation presentation)
    {
        var parsedGroups = presentation.Groups ?? (
            !string.IsNullOrWhiteSpace(presentation.CustomGroups)
                ? presentation.CustomGroups.Split(CognitoTokenGoogleGroupsSeparator, StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>()
            );

        return new Token
        {
            Sub = presentation.Sub,
            Groups = parsedGroups,
            Email = presentation.Email,
            Name = presentation.Name,
            Nbf = presentation.Nbf,
            Exp = presentation.Exp,
            Iat = presentation.Iat
        };
    }

    internal static CognitoM2MToken MapCognitoM2MToken(CognitoM2MTokenPresentation presentation)
    {
        var scopes = string.IsNullOrWhiteSpace(presentation.Scope)
            ? new List<CognitoM2MAccessScope>()
            : presentation.Scope
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(scope => TryParseAccessScope(scope, out var parsedScope) ? parsedScope : null)
                .Where(scope => scope is not null)
                .Select(scope => scope!)
                .ToList();

        return new CognitoM2MToken
        {
            ClientId = presentation.ClientId,
            Scopes = scopes
        };
    }

    private static bool TryParseAccessScope(string scope, out CognitoM2MAccessScope parsedScope)
    {
        // TODO: factor out the separators
        var separatorIndex = scope.IndexOf('/');
        var accessTypeSeparatorIndex = scope.LastIndexOf('.');

        if (separatorIndex <= 0 || scope.IndexOf('/', separatorIndex + 1) >= 0 ||
            accessTypeSeparatorIndex <= separatorIndex + 1 || accessTypeSeparatorIndex == scope.Length - 1)
        {
            parsedScope = null!;
            return false;
        }

        parsedScope = new CognitoM2MAccessScope
        {
            ApiGatewayId = scope[..separatorIndex],
            EndpointName = scope[(separatorIndex + 1)..accessTypeSeparatorIndex],
            AccessType = scope[(accessTypeSeparatorIndex + 1)..]
        };
        return true;
    }
}