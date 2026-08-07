@{
    Host = '192.168.1.12'
    User = 'artur'
    SshKeyPath = 'C:\Users\Dejsving\.ssh\local'
    RemoteTmpDir = '/tmp'
    SshOptions = @(
        '-o', 'BatchMode=yes'
    )
}
