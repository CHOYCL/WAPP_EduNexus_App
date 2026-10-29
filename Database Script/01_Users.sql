CREATE TABLE [dbo].[Users] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Username]       NVARCHAR (50)  NOT NULL,
    [Email]          NVARCHAR (100) NOT NULL,
    [PasswordHash]   NVARCHAR (100) NOT NULL,
    [Role]           NVARCHAR (20)  DEFAULT ('Student') NOT NULL,
    [PhoneNumber]    NCHAR (20)     NULL,
    [ProfilePicture] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    UNIQUE NONCLUSTERED ([Username] ASC),
    UNIQUE NONCLUSTERED ([Email] ASC)
);

