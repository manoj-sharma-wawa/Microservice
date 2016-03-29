﻿CREATE PROCEDURE [dbo].[MondayMorningBlues_ResolveByRef]
	@Email NVARCHAR(150),
	@Id BIGINT OUTPUT,
	@VersionId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
	SELECT @Id=Id, @VersionId=VersionId FROM [dbo].[MondayMorningBlues] 
	WHERE [Email] IS NOT NULL AND [Email]=@Email

	IF (@@ROWCOUNT>0)
		RETURN 200;
	--Not found.
	RETURN 404;

END