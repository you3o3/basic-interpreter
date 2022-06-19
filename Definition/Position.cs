public class Position
{
    public int idx, line, col;
    public string fileName, fileText;

    public Position(int idx, int line, int col, string fileName, string fileText)
    {
        this.idx = idx;
        this.line = line;
        this.col = col;
        this.fileName = fileName;
        this.fileText = fileText;
    }

    public Position Advance(char currCh = Constant.nullCh)
    {
        idx++;
        col++;

        if (currCh == '\n')
        {
            idx += 1;
            col = 0;
        }
        return this;
    }

    public Position Copy()
    {
        return new Position(idx, line, col, fileName, fileText);
    }
}
