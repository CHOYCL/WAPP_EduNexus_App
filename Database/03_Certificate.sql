CREATE TABLE [dbo].[Certificates] (
    [CertificateID]     INT  NOT NULL,
    [UserID]            INT  NULL,
    [courseID]          INT  NULL,
    [CertificateNumber] INT  IDENTITY (1, 1) NOT NULL,
    [DateCreated]       DATE NULL,
    PRIMARY KEY CLUSTERED ([CertificateID] ASC),
    CONSTRAINT [FK_Certificates_ToTable] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_Certificates_ToTable_1] FOREIGN KEY ([courseID]) REFERENCES [dbo].[CourseDetailTable] ([courseID])
);

