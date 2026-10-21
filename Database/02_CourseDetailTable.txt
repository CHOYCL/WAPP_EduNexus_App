CREATE TABLE [dbo].[CourseDetailTable] (
    [courseID]      INT            IDENTITY (1, 1) NOT NULL,
    [courseTitle]   NVARCHAR (50)  NOT NULL,
    [level]         NVARCHAR (50)  NOT NULL,
    [Instructor]    VARCHAR (50)   NULL,
    [Price]         FLOAT (53)     NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [EnrollAmt]     INT            NULL,
    [Thumbnail_img] NVARCHAR (MAX) NULL,
    [Category]      NVARCHAR (50)  NULL,
    PRIMARY KEY CLUSTERED ([courseID] ASC)
);

