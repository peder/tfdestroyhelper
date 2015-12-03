# tfdestroyhelper
Simplifies deleting old development branches in TFS by wrapping and automating "tf destroy" commands

Arguments:
`PurgeTFS.exe [My local TFS mapped directory] [Server TFS path] [Valid vate]`

Example usage:
`PurgeTFS.exe c:\Projects $/MyProjects/MyBranches/ 2015-09-01`

Deletes all folders at the specified level, but will not hunt recursively for old folders.
