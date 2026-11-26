-- =====================================================================================
-- Función: fn_next_turn_description
-- Base de datos: PostgreSQL
--
-- Devuelve la descripción del siguiente turno del día para un tipo de atención.
-- Formato: 2 letras del tipo de atención + 4 números secuenciales.
--   Ejemplo: AC0001 (Atención al cliente), PS0001 (Pago de servicio)
--
-- El secuencial se reinicia cada día (hora de Ecuador) y es independiente por tipo.
-- La aplica la migración "MenuSeedAndTurnFunction" y la usa el endpoint POST /api/turns.
-- =====================================================================================
CREATE OR REPLACE FUNCTION fn_next_turn_description(p_attention_type_id varchar)
RETURNS varchar
LANGUAGE plpgsql
AS $$
DECLARE
    v_today date := (now() AT TIME ZONE 'America/Guayaquil')::date;
    v_next  integer;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM attentiontype WHERE attention_type_id = p_attention_type_id) THEN
        RAISE EXCEPTION 'El tipo de atención % no existe', p_attention_type_id;
    END IF;

    -- Bloqueo por tipo de atención hasta el fin de la transacción:
    -- evita que dos gestores obtengan el mismo número al asignar turnos a la vez.
    PERFORM pg_advisory_xact_lock(hashtext('turn_' || p_attention_type_id));

    -- Se consideran también los turnos eliminados lógicamente para no reutilizar números.
    SELECT COALESCE(MAX(RIGHT(description, 4)::integer), 0) + 1
      INTO v_next
      FROM turn
     WHERE attention_type_id = p_attention_type_id
       AND (date AT TIME ZONE 'America/Guayaquil')::date = v_today;

    IF v_next > 9999 THEN
        RAISE EXCEPTION 'Se alcanzó el máximo de turnos del día para el tipo %', p_attention_type_id;
    END IF;

    RETURN p_attention_type_id || LPAD(v_next::text, 4, '0');
END;
$$;

-- Ejemplo de uso:
--   SELECT fn_next_turn_description('AC');
