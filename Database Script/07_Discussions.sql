CREATE TABLE [dbo].[DiscussionTable] (
    [DiscussionID] INT IDENTITY (1, 1) NOT NULL,
    [courseID]     INT            NOT NULL,
    [UserID]       INT            NOT NULL,
    [Title]        NVARCHAR (120) NOT NULL,
    [Details]      NVARCHAR (2000) NOT NULL,
    [Status]       NVARCHAR (20)  DEFAULT ('Open') NOT NULL,
    [CreatedAt]    DATETIME2 (0)  DEFAULT (SYSUTCDATETIME()) NOT NULL,
    PRIMARY KEY CLUSTERED ([DiscussionID] ASC),
    CONSTRAINT [CK_DiscussionTable_Status]
        CHECK ([Status] IN ('Open', 'Answered')),
    CONSTRAINT [FK_DiscussionTable_Course]
        FOREIGN KEY ([courseID]) REFERENCES [dbo].[CourseDetailTable] ([courseID]),
    CONSTRAINT [FK_DiscussionTable_User]
        FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id])
);

CREATE TABLE [dbo].[DiscussionReplyTable] (
    [ReplyID]      INT IDENTITY (1, 1) NOT NULL,
    [DiscussionID] INT            NOT NULL,
    [UserID]       INT            NOT NULL,
    [ReplyText]    NVARCHAR (2000) NOT NULL,
    [CreatedAt]    DATETIME2 (0)  DEFAULT (SYSUTCDATETIME()) NOT NULL,
    PRIMARY KEY CLUSTERED ([ReplyID] ASC),
    CONSTRAINT [FK_DiscussionReplyTable_Discussion]
        FOREIGN KEY ([DiscussionID]) REFERENCES [dbo].[DiscussionTable] ([DiscussionID]) ON DELETE CASCADE,
    CONSTRAINT [FK_DiscussionReplyTable_User]
        FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([Id])
);
