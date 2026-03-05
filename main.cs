using System;
using System.Text;

public class Program
{
    public static void Main()
    {
        try
        {
            Console.Write("Enter size matrix: ");
            string? input;
            input = Console.ReadLine();

            if (!int.TryParse(input, out int size) || size <= 0)
            {
                throw new MatrixSizeException("Size matrix must be a positive integer");
            }

            SquareMatrix matrixA;
            SquareMatrix matrixB;
            matrixA = new SquareMatrix(size, true);
            matrixB = new SquareMatrix(size, true);

            Console.WriteLine();
            Console.Write($"""
                Matrix A:
                {matrixA}
                Matrix B:
                {matrixB}
                A + B:
                {matrixA + matrixB}
                A * B:
                {matrixA * matrixB}
                det(A) = {matrixA.Determinant():F4}
                """);

            // Проверяет, существует ли обратная матрица
            if (matrixA)
            {
                Console.WriteLine();
                Console.Write($"""
                    Matrix A is invertible:
                    {matrixA.Inverse()}
                    """);
            }

            else
            {
                Console.WriteLine("Matrix A is not invertible");
            }

            Console.WriteLine();
            Console.Write($"""
                A == B : {matrixA == matrixB}
                A > B : {matrixA > matrixB}
                """);
        }

        catch (MatrixException exception)
        {
            Console.WriteLine($"Error: {exception.Message}");
        }

        catch (Exception exception)
        {
            Console.WriteLine($"Unexpected error: {exception.Message}");
        }
    }
}

public class MatrixException : Exception
{
    public MatrixException(string message) : base(message) { }
}

public class MatrixSizeException : MatrixException
{
    public MatrixSizeException(string message) : base(message) { }
}

public class MatrixOperationException : MatrixException
{
    public MatrixOperationException(string message) : base(message) { }
}

public class SquareMatrix : ICloneable, IComparable<SquareMatrix>
{
    private readonly double[,] elements;
    private readonly int dimension;
    private static readonly Random random = new Random();
    private static readonly double ZeroTolerance = 1e-9;
    private static readonly int HashPrime = 31;
    private static readonly int RandomMinValue = -5;
    private static readonly int RandomMaxValue = 6;

    public int Size
    {
        get
        {
            return dimension;
        }
    }

    public SquareMatrix(int size, bool generateRandom)
    {
        if (size <= 0)
        {
            throw new MatrixSizeException("Size matrix must be greater than zero");
        }

        dimension = size;
        elements = new double[size, size];

        if (generateRandom)
        {
            for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
                {
                    elements[rowIndex, columnIndex] = random.Next(RandomMinValue, RandomMaxValue);
                }
            }
        }
    }

    public SquareMatrix(double[,] source)
    {
        if (source is null)
        {
            throw new MatrixOperationException("Source matrix is null");
        }

        int rows;
        int columns;
        rows = source.GetLength(0);
        columns = source.GetLength(1);

        if (rows != columns)
        {
            throw new MatrixSizeException("Matrix must be square");
        }

        dimension = rows;
        elements = (double[,])source.Clone();
    }

    public double this[int rowIndex, int columnIndex]
    {
        get
        {
            ValidateIndex(rowIndex, columnIndex);
            return elements[rowIndex, columnIndex];
        }
        set
        {
            ValidateIndex(rowIndex, columnIndex);
            elements[rowIndex, columnIndex] = value;
        }
    }

    private void ValidateIndex(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= dimension ||
            columnIndex < 0 || columnIndex >= dimension)
        {
            throw new MatrixOperationException("Index goes beyond matrix bounds");
        }
    }

    public static SquareMatrix operator +(SquareMatrix first, SquareMatrix second)
    {
        if (first is null || second is null)
        {
            throw new MatrixOperationException("Matrix is null");
        }

        EnsureSameSize(first, second);
        SquareMatrix result;
        result = new SquareMatrix(first.dimension, false);

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                result[rowIndex, columnIndex] =
                    first[rowIndex, columnIndex] + second[rowIndex, columnIndex];
            }
        }

        return result;
    }

    public static SquareMatrix operator *(SquareMatrix first, SquareMatrix second)
    {
        if (first is null || second is null)
        {
            throw new MatrixOperationException("Matrix is null");
        }

        EnsureSameSize(first, second);
        SquareMatrix result;
        result = new SquareMatrix(first.dimension, false);

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                double sum = 0;

                for (int innerIndex = 0; innerIndex < first.dimension; ++innerIndex)
                {
                    sum += first[rowIndex, innerIndex] *
                           second[innerIndex, columnIndex];
                }

                result[rowIndex, columnIndex] = sum;
            }
        }

        return result;
    }

// Сравнение матриц по их определителю
public static bool operator >(SquareMatrix? first, SquareMatrix? second)
{
    if (first is null || second is null)
    {
        throw new MatrixOperationException("Cannot compare null matrices");
    }
        
    return first.CompareTo(second) > 0;
}

public static bool operator <(SquareMatrix? first, SquareMatrix? second)
{
    if (first is null || second is null)
    {
        throw new MatrixOperationException("Cannot compare null matrices");
    }
        
    return first.CompareTo(second) < 0;
}

public static bool operator >=(SquareMatrix? first, SquareMatrix? second)
{
    if (first is null || second is null)
    {
        throw new MatrixOperationException("Cannot compare null matrices");
    }

    return first.CompareTo(second) >= 0;
}

    public static bool operator <=(SquareMatrix? first, SquareMatrix? second)
    {
        if (first is null || second is null)
        {
            throw new MatrixOperationException("Cannot compare null matrices");
        }
            
        return first.CompareTo(second) <= 0;
    }

    // Проверка равенства матриц с учётом погрешности
    public static bool operator ==(SquareMatrix? first, SquareMatrix? second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first is null || second is null)
        {
            return false;
        }

        if (first.dimension != second.dimension)
        {
            return false;
        }

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                if (Math.Abs(first[rowIndex, columnIndex] -
                             second[rowIndex, columnIndex]) > ZeroTolerance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool operator !=(SquareMatrix? first, SquareMatrix? second)
    {
        return !(first == second);
    }

    // Позволяет использовать матрицу в условии if (matrix)
    public static bool operator true(SquareMatrix matrix)
    {
        if (matrix is null)
        {
            return false;
        }

        return Math.Abs(matrix.Determinant()) > ZeroTolerance;
    }

    public static bool operator false(SquareMatrix matrix)
    {
        if (matrix is null)
        {
            return true;
        }

        return Math.Abs(matrix.Determinant()) <= ZeroTolerance;
    }

    public static explicit operator double(SquareMatrix matrix)
    {
        return matrix.Determinant();
    }

    private static void EnsureSameSize(SquareMatrix first, SquareMatrix second)
    {
        if (first.dimension != second.dimension)
        {
            throw new MatrixOperationException("Matrices must be same size");
        }
    }

// Вычисление определителя методом Гаусса
public double Determinant()
{
    double[,] copy;
    double determinant;
    int swapCount;
    copy = (double[,])elements.Clone();
    determinant = 1;
    swapCount = 0;

    for (int pivotIndex = 0; pivotIndex < dimension; ++pivotIndex)
    {
        int maxRow;
        maxRow = pivotIndex;

        for (int rowIndex = pivotIndex + 1; rowIndex < dimension; ++rowIndex)
        {
            if (Math.Abs(copy[rowIndex, pivotIndex]) >
                Math.Abs(copy[maxRow, pivotIndex]))
            {
                maxRow = rowIndex;
            }
        }

        if (Math.Abs(copy[maxRow, pivotIndex]) <= ZeroTolerance)
        {
            return 0;
        }

        // Перестановка строк
        if (maxRow != pivotIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                double temp;
                temp = copy[pivotIndex, columnIndex];
                copy[pivotIndex, columnIndex] = copy[maxRow, columnIndex];
                copy[maxRow, columnIndex] = temp;
            }

            ++swapCount;
        }

        for (int rowIndex = pivotIndex + 1; rowIndex < dimension; ++rowIndex)
        {
            double factor;
            factor = copy[rowIndex, pivotIndex] / copy[pivotIndex, pivotIndex];

            for (int columnIndex = pivotIndex; columnIndex < dimension; ++columnIndex)
            {
                copy[rowIndex, columnIndex] -=
                    factor * copy[pivotIndex, columnIndex];
            }
        }
    }

    for (int index = 0; index < dimension; ++index)
    {
        determinant *= copy[index, index];
    }

    if (swapCount % 2 != 0)
    {
        determinant = -determinant;
    }

    return determinant;
}

    // Создание минора подматрицы без указанной строки и столбца
    private SquareMatrix CreateMinor(int excludedRow, int excludedColumn)
    {
        SquareMatrix minor;
        int minorRow;
        minor = new SquareMatrix(dimension - 1, false);
        minorRow = 0;

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            if (rowIndex == excludedRow)
            {
                continue;
            }

            int minorColumn;

            minorColumn = 0;

            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                if (columnIndex == excludedColumn)
                {
                    continue;
                }
                
                minor[minorRow, minorColumn] = elements[rowIndex, columnIndex];
                ++minorColumn;
            }

            ++minorRow;
        }

        return minor;
    }

    // Вычисление обратной матрицы через алгебраические дополнения
    public SquareMatrix Inverse()
    {   
        if (dimension == 1)
        {
            if (Math.Abs(elements[0, 0]) <= ZeroTolerance)
            {
                throw new MatrixOperationException("Matrix is degenerate");
            }

            SquareMatrix result = new SquareMatrix(1, false);
            result[0, 0] = 1 / elements[0, 0];
            return result;
        }

        double determinant;
        determinant = Determinant();

        if (Math.Abs(determinant) <= ZeroTolerance)
        {
            throw new MatrixOperationException("Matrix is degenerate");
        }

        SquareMatrix adjugate;
        adjugate = new SquareMatrix(dimension, false);

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                SquareMatrix minor = CreateMinor(rowIndex, columnIndex);
                double sign = (rowIndex + columnIndex) % 2 == 0 ? 1 : -1;

                adjugate[columnIndex, rowIndex] =
                    sign * minor.Determinant() / determinant;
            }
        }

        return adjugate;
    }

    public SquareMatrix CloneMatrix()
    {
        return new SquareMatrix((double[,])elements.Clone());
    }

    object ICloneable.Clone()
    {
        return CloneMatrix();
    }

    public int CompareTo(SquareMatrix? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Determinant().CompareTo(other.Determinant());
    }

    public override bool Equals(object? obj)
    {
        return obj is SquareMatrix matrix && this == matrix;
    }

    public override int GetHashCode()
    {
        int hash;
        hash = dimension;

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                double value = Math.Abs(elements[rowIndex, columnIndex]) < ZeroTolerance
                    ? 0
                    : elements[rowIndex, columnIndex];

                hash = hash * HashPrime + value.GetHashCode();
            }
        }

        return hash;
    }

    public override string ToString()
    {
        StringBuilder builder;
        builder = new StringBuilder();

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                double value = Math.Abs(elements[rowIndex, columnIndex]) < ZeroTolerance
                    ? 0
                    : elements[rowIndex, columnIndex];

                builder.Append($"{value, 8:F2}");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}