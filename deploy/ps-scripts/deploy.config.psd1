@{
    Host = '192.168.1.14'
    User = 'artur'
    SshKeyPath = 'C:\Users\Dejsving\.ssh\local'
    RemoteTmpDir = '/tmp'
    SshOptions = @(
        '-o', 'BatchMode=yes'
    )
}
