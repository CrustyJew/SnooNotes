update notetypes
set ColorCode = (LEFT(ColorCode,1)+Left(ColorCode,1)+SUBSTRING(ColorCode,2,1)+SUBSTRING(ColorCode,2,1)+RIGHT(ColorCode,1)+RIGHT(ColorCode,1))
where LEN(ColorCode) = 3;
GO;

