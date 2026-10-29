CREATE TABLE [dbo].[MaterialTable] (
    [MaterialID]          INT            IDENTITY (1, 1) NOT NULL,
    [courseID]            INT            NOT NULL,
    [materialType]        NVARCHAR (MAX) NULL,
    [materialURL]         NVARCHAR (MAX) NULL,
    [materialDescription] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([MaterialID] ASC),
    CONSTRAINT [FK_MaterialTable_ToTable] FOREIGN KEY ([courseID]) REFERENCES [dbo].[CourseDetailTable] ([courseID])
);

