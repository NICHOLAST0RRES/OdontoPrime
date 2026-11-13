namespace OdontoPrime.Application;

public class HorarioOcupadoException(Exception causa)
    : Exception("Profissional já tem consulta nesse horário.", causa);
    
