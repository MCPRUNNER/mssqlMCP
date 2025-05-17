# Security Checklist

## Initial Setup

- [ ] Set a strong encryption key using `MSSQL_MCP_KEY` environment variable
- [ ] Start the server with encryption enabled using `Start-MCP-Encrypted.ps1`
- [ ] Save the encryption key in a secure location (not in source code)

## Regular Maintenance

- [ ] Rotate encryption key every 90 days using `Rotate-Encryption-Key.ps1`
- [ ] Run `Verify-Encryption-Status.ps1` monthly to ensure proper encryption
- [ ] Migrate any unencrypted connections using `Migrate-To-Encrypted.ps1`
- [ ] Monitor logs for any encryption or decryption errors

## Development Best Practices

- [ ] Never hardcode encryption keys in source code
- [ ] Use environment variables for all sensitive information
- [ ] Run unit tests regularly to verify security features
- [ ] Use SSL/TLS for all production connections

## Production Deployment

- [ ] Use a secrets manager for storing the encryption key
- [ ] Set up automatic key rotation in your CI/CD pipeline
- [ ] Configure proper access controls for the SQL Server MCP server
- [ ] Enable HTTPS for all API communication
- [ ] Implement proper authentication for all endpoints

## Security Incident Response

- [ ] If a key is compromised, immediately rotate to a new key
- [ ] Document any security incidents
- [ ] Review logs to determine the extent of any exposure
- [ ] Update security procedures based on incident findings

## Regular Testing

- [ ] Run `Test-Security-Features.ps1` after any system changes
- [ ] Perform penetration testing on the API
- [ ] Validate encryption status with `Assess-Connection-Security.ps1`
- [ ] Test key rotation in non-production environments first

## Documentation

- [ ] Keep a record of key rotation dates
- [ ] Document all security configurations
- [ ] Maintain a list of all connection strings and their encryption status
- [ ] Create procedures for key management and incident response
