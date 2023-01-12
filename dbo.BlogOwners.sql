CREATE TABLE [dbo].[BlogOwners] (
    [UserId]        INT           NOT NULL,
    [BlogId]        INT           NOT NULL,
    [OwnerName]     NVARCHAR (50) NOT NULL,
    [IsOwnerExists] BIT           NOT NULL,
    CONSTRAINT [PK_BlogOwners] PRIMARY KEY CLUSTERED ([BlogId] ASC),
	CONSTRAINT [FK_BlogOwners_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]),
    CONSTRAINT [FK_BlogOwners_Blogs_BlogId] FOREIGN KEY ([BlogId]) REFERENCES [dbo].[Blogs] ([Id]) ON DELETE CASCADE
);


