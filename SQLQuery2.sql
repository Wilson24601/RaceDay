SET IDENTITY_INSERT Users ON;

INSERT INTO Users (UserId, FirstName, LastName, Email, PasswordHash, Role, Phone) 
VALUES (1, 'Test', 'Organiser', 'organiser@test.com', 'dummyhash', 'Organiser', NULL);

SET IDENTITY_INSERT Users OFF;