-- 08_Quiz.sql
-- Module, learning material and quiz tables for EduNexus.
-- Run AFTER 01 to 07 (needs Users and CourseDetailTable).
--
-- If you ran an earlier draft of this script, drop the old tables first (child tables first):
-- DROP TABLE IF EXISTS [dbo].[QuizAnswerTable];
-- DROP TABLE IF EXISTS [dbo].[QuizAttemptTable];
-- DROP TABLE IF EXISTS [dbo].[QuizQuestionTable];
-- DROP TABLE IF EXISTS [dbo].[ModuleMaterialTable];
-- DROP TABLE IF EXISTS [dbo].[ModuleTable];

CREATE TABLE [dbo].[ModuleTable] (
    [moduleID]          INT            IDENTITY (1, 1) NOT NULL,
    [courseID]          INT            NOT NULL,
    [moduleTitle]       NVARCHAR (150) NOT NULL,
    [moduleDescription] NVARCHAR (MAX) NULL,
    [sortOrder]         INT            NOT NULL DEFAULT (1),
    [passPercentage]    INT            NOT NULL DEFAULT (50),
    PRIMARY KEY CLUSTERED ([moduleID] ASC),
    FOREIGN KEY ([courseID]) REFERENCES [dbo].[CourseDetailTable] ([courseID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[ModuleMaterialTable] (
    [moduleMaterialID] INT            IDENTITY (1, 1) NOT NULL,
    [moduleID]         INT            NOT NULL,
    [materialTitle]    NVARCHAR (150) NOT NULL,
    [materialKind]     NVARCHAR (10)  NOT NULL CHECK ([materialKind] IN ('Video', 'Reading')),
    [filePath]         NVARCHAR (500) NULL,   -- uploaded MP4 or PDF
    [readingContent]   NVARCHAR (MAX) NULL,   -- typed reading text
    [sortOrder]        INT            NOT NULL DEFAULT (1),
    PRIMARY KEY CLUSTERED ([moduleMaterialID] ASC),
    FOREIGN KEY ([moduleID]) REFERENCES [dbo].[ModuleTable] ([moduleID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[QuizQuestionTable] (
    [questionID]    INT            IDENTITY (1, 1) NOT NULL,
    [moduleID]      INT            NOT NULL,
    [questionType]  NVARCHAR (10)  NOT NULL DEFAULT ('MCQ')
                    CHECK ([questionType] IN ('MCQ', 'Short', 'Long')),
    [questionText]  NVARCHAR (500) NOT NULL,
    [optionA]       NVARCHAR (250) NULL,
    [optionB]       NVARCHAR (250) NULL,
    [optionC]       NVARCHAR (250) NULL,
    [optionD]       NVARCHAR (250) NULL,
    [correctOption] CHAR (1)       NULL CHECK ([correctOption] IN ('A', 'B', 'C', 'D')),
    [modelAnswer]   NVARCHAR (MAX) NULL,   -- Short: accepted answers split by |  /  Long: marking guide
    [marks]         INT            NOT NULL DEFAULT (1) CHECK ([marks] > 0),
    PRIMARY KEY CLUSTERED ([questionID] ASC),
    FOREIGN KEY ([moduleID]) REFERENCES [dbo].[ModuleTable] ([moduleID]) ON DELETE CASCADE,
    CONSTRAINT [CK_QuizQuestion_TypeFields] CHECK (
           ([questionType] = 'MCQ'
                AND [optionA] IS NOT NULL AND [optionB] IS NOT NULL
                AND [correctOption] IS NOT NULL)
        OR ([questionType] = 'Short' AND [modelAnswer] IS NOT NULL)
        OR ([questionType] = 'Long')
    )
);

CREATE TABLE [dbo].[QuizAttemptTable] (
    [attemptID]   INT           IDENTITY (1, 1) NOT NULL,
    [UserID]      INT           NOT NULL,
    [moduleID]    INT           NOT NULL,
    [attemptNo]   INT           NOT NULL CHECK ([attemptNo] BETWEEN 1 AND 3),
    [totalMarks]  INT           NOT NULL,
    [score]       DECIMAL (6,2) NOT NULL DEFAULT (0),   -- marks awarded so far
    [status]      NVARCHAR (10) NOT NULL DEFAULT ('Graded')
                  CHECK ([status] IN ('Graded', 'Pending')),
    [percentage]  DECIMAL (5,2) NULL,                   -- NULL while Pending
    [grade]       NVARCHAR (2)  NULL,                   -- NULL while Pending
    [isPassed]    BIT           NULL,                   -- NULL while Pending
    [attemptDate] DATETIME      NOT NULL DEFAULT (GETDATE()),
    [gradedDate]  DATETIME      NULL,
    PRIMARY KEY CLUSTERED ([attemptID] ASC),
    UNIQUE ([UserID], [moduleID], [attemptNo]),
    FOREIGN KEY ([UserID])   REFERENCES [dbo].[Users] ([Id])             ON DELETE CASCADE,
    FOREIGN KEY ([moduleID]) REFERENCES [dbo].[ModuleTable] ([moduleID]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[QuizAnswerTable] (
    [answerID]       INT            IDENTITY (1, 1) NOT NULL,
    [attemptID]      INT            NOT NULL,
    [questionID]     INT            NOT NULL,
    [selectedOption] CHAR (1)       NULL,    -- MCQ
    [answerText]     NVARCHAR (MAX) NULL,    -- Short / Long
    [marksAwarded]   DECIMAL (5,2)  NULL,    -- NULL = waiting for admin to mark
    [adminFeedback]  NVARCHAR (500) NULL,
    PRIMARY KEY CLUSTERED ([answerID] ASC),
    UNIQUE ([attemptID], [questionID]),
    FOREIGN KEY ([attemptID])  REFERENCES [dbo].[QuizAttemptTable]  ([attemptID])  ON DELETE CASCADE,
    FOREIGN KEY ([questionID]) REFERENCES [dbo].[QuizQuestionTable] ([questionID])
);