set now=%date:~-4%_%date:~3,2%_%date:~0,2%__%time:~0,2%_%time:~3,2%_%time:~6,2%
dotnet ef migrations add %now%
dotnet run clean-empty-migrations-files
dotnet ef database update