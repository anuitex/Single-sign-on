/* Migration Script */
create table Book
(
	[Id] [int] identity(1,1) not null constraint PK_Entry_Id primary key,
	[Name] [nvarchar](50) not null,
	[Title] [nvarchar](200) not null,
	[Summary] [nvarchar](max) not null,
	[IsVisible] [bit] not null,
	[Published] [datetime] not null,
	[LatestRevisionId] [int] null,
	[TUTU] [nvarchar] not null
)
