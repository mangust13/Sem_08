exec sp_configure 'show advanced options', '1';
go
reconfigure;
go
exec sp_configure 'user connections', '10';
go
reconfigure;
go