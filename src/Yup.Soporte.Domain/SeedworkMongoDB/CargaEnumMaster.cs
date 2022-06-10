using System.Collections.Generic;
using System.Linq;

namespace Yup.Soporte.Domain.SeedworkMongoDB;

public enum TipoCargaServicioExterno
{
    NO_DEFINIDO = 0,
    STUDENTS = 1,
}

public enum TipoCarga
{
    NO_DEFINIDO = 0,
    STUDENTS = 1,
}

public enum EstadoCarga
{
    NO_DEFINIDO = 0,
    PENDIENTE = 10,
    EN_EVALUACION = 20,
    EVALUADO_CON_OBSERVACIONES = 21,
    EVALUADO_CORRECTO = 22,
    EN_REGISTRO = 30,
    REGISTRADO_CON_ERRORES = 31,
    REGISTRADO_CORRECTO = 32,
    FINALIZADO = 40,
    CANCELADO = 50,
    EXISTE_ERROR = 60
}

public enum OrigenCarga
{
    ARCHIVO_EXCEL = 0,
    SERVICIO_EXTERNO = 1
}

public static class CargaEnumsExtensions
{
    public static TipoCarga ParseTipoCarga(string value)
    {
        var tmpEnumVal = TipoCarga.NO_DEFINIDO;
        Enum.TryParse(value, true, out tmpEnumVal);
        return tmpEnumVal;
    }
    public static EstadoCarga ParseEstadoCarga(string value)
    {
        var tmpEnumVal = EstadoCarga.NO_DEFINIDO;
        Enum.TryParse(value, true, out tmpEnumVal);
        return tmpEnumVal;
    }
    public static TipoCarga[] ParseTipoCargaList(char separator, string csvList)
    {
        var result = new TipoCarga[0];
        #region Validacion
        if (string.IsNullOrWhiteSpace(csvList) ||
            char.IsWhiteSpace(separator))
        { return result; }
        #endregion
        var arrTiposCargaStr = csvList.Split(separator);
        result = new TipoCarga[arrTiposCargaStr.Length];
        for (var i = 0; i < arrTiposCargaStr.Length; i++)
        {
            result[i] = ParseTipoCarga(arrTiposCargaStr[i]);
        }
        return result;
    }
    public static EstadoCarga[] ParseEstadoCargaList(char separator, string csvList, bool incluirSubEstadosProceso = false)
    {
        var result = new EstadoCarga[0];
        #region Validacion
        if (string.IsNullOrWhiteSpace(csvList) ||
            char.IsWhiteSpace(separator))
        { return result; }
        #endregion
        var lstEstadosCargaStr = csvList.Split(separator).ToList();
        lstEstadosCargaStr = lstEstadosCargaStr.Select(x => x.Trim()).Distinct().ToList();
        var lstEstadosCarga = new List<EstadoCarga>();
        foreach (var strId in lstEstadosCargaStr)
        {
            var tmpIntId = 0;
            if (int.TryParse(strId, out tmpIntId))
            {
                if (Enum.IsDefined(typeof(EstadoCarga), tmpIntId))
                {
                    lstEstadosCarga.Add((EstadoCarga)tmpIntId);
                }

                if (incluirSubEstadosProceso)
                {
                    var baseGrupo = (int)(tmpIntId * 0.1) * 10;
                    for (var i = 0; i < 9; i++)
                    {
                        if (baseGrupo + i > 0 &&
                            Enum.IsDefined(typeof(EstadoCarga), baseGrupo + i))
                        {
                            lstEstadosCarga.Add((EstadoCarga)(baseGrupo + i));
                        }
                    }
                }
            }
        }
        lstEstadosCarga = lstEstadosCarga.Distinct().ToList();

        return lstEstadosCarga.ToArray();
    }
    public static EstadoCarga ObtenerEstadoBase(EstadoCarga estado)
    {
        if (estado == EstadoCarga.NO_DEFINIDO) return estado;

        var result = EstadoCarga.NO_DEFINIDO;
        var intEstado = (int)estado;
        var intEstadoBase = (int)(intEstado * 0.1) * 10;
        if (intEstadoBase > 0 &&
                            Enum.IsDefined(typeof(EstadoCarga), intEstadoBase))
        {
            result = (EstadoCarga)intEstadoBase;
        }

        return result;

    }
}
