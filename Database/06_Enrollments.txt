CREATE TABLE [dbo].[Enrollments] (
    [EnrollmentID]   INT        NOT NULL,
    [UserID]         INT        NULL,
    [courseID]       INT        NULL,
    [EnrollmentDate] DATE       NULL,
    [Status]         NCHAR (20) NULL,
    PRIMARY KEY CLUSTERED ([EnrollmentID] ASC),
    CONSTRAINT [FK_Enrollments_ToTable] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id])
);

