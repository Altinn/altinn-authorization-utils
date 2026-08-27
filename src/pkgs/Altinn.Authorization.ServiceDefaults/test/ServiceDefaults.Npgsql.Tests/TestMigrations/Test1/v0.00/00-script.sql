CREATE FUNCTION app.test_function (value BIGINT)
RETURNS BIGINT
AS $$
BEGIN
    RETURN value + 1;
END;
$$ LANGUAGE plpgsql;
