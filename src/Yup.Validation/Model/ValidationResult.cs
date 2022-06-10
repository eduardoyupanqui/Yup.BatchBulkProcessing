using System.Collections.Generic;

namespace Yup.Validation
{
    public class EmptyValidationResult : ValidationResult
    {
        public EmptyValidationResult() : base()
        {
            base.EsElementoVacio = true;
            base.Observaciones.Add("Fila vacía");
        }
    }

    public class ValidationResult
    {
        public ValidationResult(string groupKey) : this()
        {
            this.GroupKey = groupKey;
        }
        public ValidationResult()
        {
            Observaciones = new List<string>();
            FilaObservaciones = new List<ItemInnerResult>();
            InnerResults = new List<ValidationResult>();
        }

        public bool EsElementoVacio { get; protected set; }
        public bool EsValido
        {
            get
            {
                return Observaciones.Count == 0;
            }
            set
            {

            }
        }
        public List<string> Observaciones { get; set; }
        public List<ItemInnerResult> FilaObservaciones { get; set; }

        public override string ToString()
        {
            return string.Join(",", Observaciones);
        }

        public string GroupKey { get; protected set; }
        public List<ValidationResult> InnerResults { get; set; }
    }

    public class ItemInnerResult {

        public ItemInnerResult (int nroFila, string observacion)
        {
            NroFila = nroFila;
            Observacion = observacion;
        }

        public int NroFila { get; private set; }
        public string Observacion { get; private set; }
    }
}