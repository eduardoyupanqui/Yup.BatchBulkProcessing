using Yup.Validation;
using System;
using System.Collections.Generic;
using Yup.AspNetCore.Domain.Base.Entities;

namespace Yup.Student.Domain.AggregatesModel.StudentAggregate;

public class Student : Entity, IAggregateRoot, IValidable
{
    [ValorRequerido]
    public string TipoDocumento { get; private set; }
    [ValorRequerido]
    public string NroDocumento { get; private set; }
    public string LenguaNativa { get; private set; }
    public string IdiomaExtranjero { get; private set; }
    public string CondicionDiscapacidad { get; private set; }
    public string CodigoORCID { get; private set; }
    //public string CorreoPersonal { get; private set; }
    //public string CorreoInstitucional { get; private set; }
    public string UbigeoDomicilio { get; private set; }

    public int NumeroFila { get; set; }

    protected Student() { }
    public Student(
        string tipoDocumento, 
        string nroDocumento, 
        string lenguaNativa, 
        string idiomaExtranjero, 
        string condicionDiscapacidad, 
        string codigoORCID, 
        string ubigeoDomicilio, 
        int numeroFila,
        Guid usuarioRegistro,
        DateTime fechaRegistro,
        string ipRegistro
        ) : this()
    {
        TipoDocumento = tipoDocumento;
        NroDocumento = nroDocumento;
        LenguaNativa = lenguaNativa;
        IdiomaExtranjero = idiomaExtranjero;
        CondicionDiscapacidad = condicionDiscapacidad;
        CodigoORCID = codigoORCID;
        UbigeoDomicilio = ubigeoDomicilio;

        EsEliminado = false;
        this.UsuarioCreacion = usuarioRegistro;
        this.FechaCreacion = fechaRegistro;
        this.IpCreacion = ipRegistro;
        NumeroFila = numeroFila;
    }
    public Student(
       string tipoDocumento,
       string nroDocumento,
       string lenguaNativa,
       string idiomaExtranjero,
       string condicionDiscapacidad,
       string codigoORCID,
       string ubigeoDomicilio,
       Guid usuarioRegistro,
       DateTime fechaRegistro,
       string ipRegistro
       ) : this()
    {
        TipoDocumento = tipoDocumento;
        NroDocumento = nroDocumento;
        LenguaNativa = lenguaNativa;
        IdiomaExtranjero = idiomaExtranjero;
        CondicionDiscapacidad = condicionDiscapacidad;
        CodigoORCID = codigoORCID;
        UbigeoDomicilio = ubigeoDomicilio;

        EsEliminado = false;
        this.UsuarioCreacion = usuarioRegistro;
        this.FechaCreacion = fechaRegistro;
        this.IpCreacion = ipRegistro;
    }
}
