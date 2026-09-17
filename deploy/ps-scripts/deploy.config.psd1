@{
    Host = '192.168.1.3'
    User = 'artur'
    SshKeyPath = 'C:\Users\Dejsving\.ssh\local'
    RemoteTmpDir = '/tmp'
    SshOptions = @(
        '-o', 'BatchMode=yes'
    )
}
