# Security Policy

## Reporting a Vulnerability

**Do NOT create a public GitHub issue for security vulnerabilities.** This could put all users at risk.

### Private Vulnerability Reporting

We take security seriously and appreciate your responsible disclosure. To report a security vulnerability:

1. **Use GitHub Private Vulnerability Reporting** (Recommended)
   - Navigate to https://github.com/sarmkadan/dotnet-deploy-notify/security/advisories
   - Click "Report a vulnerability"
   - Provide detailed information about the issue
   - Submit the report privately

2. **Email Report**
   - Send details to: **rutova2@gmail.com**
   - Include a descriptive title and detailed explanation
   - Specify affected versions
   - Provide steps to reproduce (if applicable)

### What to Include

Please provide the following information:

- Clear description of the vulnerability
- Affected version(s)
- Steps to reproduce
- Potential impact
- Suggested fix (if available)
- Your contact information

## Response Timeline

We commit to the following response times:

- **48 hours** - Initial acknowledgment of your report
- **1 week** - Detailed assessment and mitigation plan
- **Ongoing** - Regular updates on progress toward a fix

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x     | ✅ Yes    |
| < 1.0   | ❌ No     |

Security updates will be released for version 1.x as needed.

## Security Best Practices for Users

When using DotNetDeployNotify in your infrastructure:

1. **Webhook URLs** - Keep your webhook URLs secure and never commit them to version control
   - Use environment variables or secure configuration management
   - Rotate credentials if accidentally exposed

2. **Authentication** - Use strong authentication for webhook endpoints
   - Include authentication tokens in custom headers
   - Validate webhook signatures when possible

3. **Network Security**
   - Restrict outbound network access to trusted webhook endpoints
   - Use HTTPS for all webhook communications
   - Monitor webhook delivery logs for suspicious activity

4. **Configuration Management**
   - Never hardcode sensitive credentials in appsettings.json
   - Use environment-specific configuration files
   - Review configuration before deployment

5. **Updates** - Keep DotNetDeployNotify updated
   - Monitor releases for security patches
   - Test updates in non-production environments first
   - Apply security updates promptly

## Scope

This security policy applies to:

- DotNetDeployNotify core library and services
- Official NuGet packages and releases
- GitHub repository and issues

Out of scope:

- Third-party dependencies and libraries
- Applications or services using DotNetDeployNotify
- User configuration or deployment practices

For vulnerabilities in third-party dependencies, please report directly to the respective project maintainers.

## Acknowledgments

We appreciate responsible disclosure by security researchers and community members. If you report a vulnerability, we will acknowledge your contribution in the security advisory (unless you prefer to remain anonymous).

---

Thank you for helping keep DotNetDeployNotify secure!
