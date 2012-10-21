/* Can be deleted???

using System;
using System.Collections.Generic;
using System.Text;

namespace GadgetLoader
{
    class HsmlReader
    {
        string path;
        int curFile; // current file index reading
        int curIndex; // current index into file
        int numFiles; // total files
        HsmlFile hsmlFile; // and current data

        public HsmlReader()
        {
        }

        // path should be filename w/out ".i"
        public HsmlReader(string path)
        {
            this.path = path;
            curFile = -1;
            curIndex = 0;
            numFiles = 1;
            // and start reading
            NextFile();
        }

        // load next file in sequence
        void NextFile()
        {
            curFile++;
            // don't load if doesn't exist
            if (curFile >= numFiles)
                return;

            hsmlFile = new HsmlFile(path + "." + curFile);
            numFiles = (int)hsmlFile.numFiles;
            curIndex = 0;
        }

        public void Next()
        {
            curIndex++;
            if (curIndex == hsmlFile.numFile)
                NextFile();
        }

        public float GetHsml()
        {
            return hsmlFile.hsml[curIndex];
        }

        public float GetVelDisp()
        {
            return hsmlFile.velDisp[curIndex];
        }

        public float GetDensity()
        {
            return hsmlFile.density[curIndex];
        }
    }
}


*/