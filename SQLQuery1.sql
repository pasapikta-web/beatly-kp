IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tracks]') AND name = N'IsLiked')
BEGIN
    ALTER TABLE [dbo].[Tracks] ADD [IsLiked] BIT NOT NULL DEFAULT 0;
END