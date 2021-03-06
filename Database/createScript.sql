CREATE TABLE [Facts].[dbo].[FactEvents] (
    [FactID]                  BIGINT         IDENTITY (1, 1) NOT NULL,
    [opta_event_id]           BIGINT         NOT NULL,
    [fixture_id]              BIGINT         NOT NULL,
    [away_score]              SMALLINT       NULL,
    [home_score]              SMALLINT       NULL,
    [period]                  VARCHAR (30)   NULL,
    [timestamp]               DATETIME       NULL,
    [timestamp_milliseconds]  VARCHAR (3)    NULL,
    [period_id]               SMALLINT       NULL,
    [period_minute]           TINYINT        NULL,
    [period_second]           TINYINT        NULL,
    [player_id]               BIGINT         NULL,
    [event_id]                SMALLINT       NULL,
    [event_type_id]           SMALLINT       NULL,
    [event_type_name]         VARCHAR (30)   NULL,
    [team_id]                 BIGINT         NOT NULL,
    [outcome]                 SMALLINT       NULL,
    [game_status]             VARCHAR (30)   NULL,
    [timer_running]           TINYINT        NULL,
    [direction]               VARCHAR (13)   NULL,
    [direction_of_play_x]     DECIMAL (5, 2) NULL,
    [direction_of_play_y]     DECIMAL (5, 2) NULL,
    [x]                       DECIMAL (5, 2) NULL,
    [y]                       DECIMAL (5, 2) NULL,
    [last_modified]           DATETIME       NULL,
    [related_event_id]        INT            NULL,
    [gameAdditional]          VARCHAR (MAX)  NULL,
    [eventAdditional]         VARCHAR (MAX)  NULL,
    [created]                 DATETIME       NULL,
    PRIMARY KEY CLUSTERED ([opta_event_id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [eventIndex]
    ON [dbo].[FactEvents]([fixture_id] ASC, [event_id] ASC);


GO

CREATE TABLE [Facts].[dbo].[Qualifiers] (
    [ID]                  BIGINT        NOT NULL,
    [name]                NVARCHAR (50) NULL,
    [value]               NVARCHAR (50) NULL,
    [qualifier_id]        BIGINT        NULL,
    [event_id]            BIGINT        NULL,
    [qualifierAdditional] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_Qualifiers] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Qualifiers_FactEvents] FOREIGN KEY ([event_id]) REFERENCES [Facts].[dbo].[FactEvents] ([opta_event_id])
);

GO