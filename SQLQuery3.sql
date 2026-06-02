-- Проверяем, есть ли уже колонки, прежде чем их добавлять
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tracks') AND name = 'Genre')
    ALTER TABLE Tracks ADD Genre INT;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tracks') AND name = 'Mood')
    ALTER TABLE Tracks ADD Mood INT;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tracks') AND name = 'Album')
    ALTER TABLE Tracks ADD Album INT;
GO