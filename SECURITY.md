# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in CodeNav, please report it by creating a new security advisory. Please do not open a public issue for security vulnerabilities.

**Contact**: https://github.com/sboulema/CodeNav/security/advisories/new

We take security seriously and will respond to valid reports as quickly as possible. Please include:

- A description of the vulnerability
- Steps to reproduce the issue
- Potential impact assessment
- Any suggested fixes (if applicable)

### Dependencies

CodeNav has minimal dependencies:

- **.NET 8**
- **Microsoft.VisualStudio.Extensibility.Sdk 17.14.40608** - Used for extending Visual Studio (Out-of-Proc)
- **Microsoft.VisualStudio.SDK 17.14.40265** - Used for extending Visual Studio (In-Proc)
- **Microsoft.VisualStudio.Extensibility.Build 17.14.40608** - Used for building the extension
- **Microsoft.CodeAnalysis.CSharp 5.0.0** - Used for parsing C# code

All dependencies are from trusted sources and regularly updated.

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 10.x.x  | :white_check_mark: |
| 9.x.x   | :x: |

## Security Updates

Security updates will be released as patch versions. We recommend:

- Always use the latest patch version
- Subscribe to repository releases for notifications
- Review the releases changelogs for security-related updates

## Acknowledgments

We appreciate security researchers who responsibly disclose vulnerabilities. Contributors will be acknowledged in release notes (unless they prefer to remain anonymous).
