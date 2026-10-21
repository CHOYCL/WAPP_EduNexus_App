CREATE TABLE [dbo].[CourseCategory] (
    [CategoryID]   INT            IDENTITY (1, 1) NOT NULL,
    [CategoryName] NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);

