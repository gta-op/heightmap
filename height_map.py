"""
Height map correction script

This script is released under the conditions of the MIT license

Author: Draex (https://forum.gtanet.work/index.php?members/draex.4963/)
"""

import numpy as np
import matplotlib.pyplot as plt


def read(file, shape, dtype='float32', mem_usage=1):
    """
    Loads data from a file as numpy array
    
    Args:
        file (string): file name
        shape (tuple): data shape (y, x), e.g. (rows*sizeY, columns*sizeX)
        dtype (string): data type (usually float32)
        mem_usage (int): 0 => use numpys memmap, 1 => load the data in the RAM
    """
    if mem_usage: return np.array(np.fromfile(file, dtype=dtype)).reshape(shape)
    return np.memmap(file, dtype=dtype, mode='r', shape=shape)

def correct(file, file2, shape, dtype='float32', mem_usage=1, visualize=False, visualize_report=False, verbose=True, tmp_file="_hmap_tmp.dat"):
    """
    Corrects the height data from file with the data from file2
    
    The data of both files will be compared. If the values differ, the data that isn't 0 will be taken.
    If the values differ and none of them is 0 the data from file will be taken.
    
    Args:
        file (string): file name for the first file
        file2 (string): file name for the second file
        shape (tuple): data shape (y, x), e.g. (rows*sizeY, columns*sizeX)
        dtype (string): data type (usually float32)
        mem_usage (int):
            0 => use numpys memmap (slow),
            1 => load the data in the RAM, use memmap for the corrected data (fast)
            2 => load the data in the RAM, do not use memmap (fastest, huge RAM load though)
        visualize (bool):
            instead of correcting the data, output correction type (int)
            Correction types:
                -1 => different values (not fixed)
                0 => equal values
                1 => different values (fixed)
                2 => both values 0f
        visualize_report (bool):
            print out the amount of each correction type
            
            Note:
                this will really slow down the script
        verbose (bool):
            use prints (be verbose)
        tmp_file:
            file name for the temporary file for memmap
            if mem_usage is > 1 the file won't be used
            
            Note:
                There no need to save data to another file after calculation when the file is used.
                You can just put the name of the output file here
    """
    
    if verbose: print("reading file %s.."%file)
    data = read(file, shape, dtype=dtype, mem_usage=mem_usage)
    
    if verbose: print("reading file %s.."%file2)
    data2 = read(file2, shape, dtype=dtype, mem_usage=mem_usage)
    
    
    if verbose: print("applying correction..")
    
    # Compare two data points (-1 => different (not fixed), 0 => equal, 1 => different (fixed), 2 => both 0f) 
    col = lambda x,y: 1+(x==y) if x*y==0 else 0-(x-y>0.01)
    
    # Compare two data points, take the data point which isn't 0 if they differ
    fixit = lambda x,y: (x if y==0 else y) if x*y==0 else x
    
    if mem_usage and not (visualize and visualize_report):
        # Create the function which will be applied to the np.array
        f = np.frompyfunc((col if visualize else fixit),2,1)
        
        # Apply the function using both data arrays
        if mem_usage>1: return f(data, data2)[:].astype(dtype, copy=False)
        else:
            tmp = np.memmap(tmp_file, dtype=dtype, mode='w+', shape=shape)
            tmp[:] = f(data, data2)[:]
            return tmp
    
    
    if mem_usage: tmp = np.empty(shape, dtype=dtype)
    else: tmp = np.memmap(tmp_file, dtype=dtype, mode='w+', shape=shape)
    
    if visualize:
        if visualize_report:
            unresolved=0
            nll=0
            fix=0
        
        # Iterate through data
        for i in range(len(data)):
            if verbose: print("%d/%d"%(i+1, shape[0]))
            
            # load row-wise (faster)
            if not mem_usage:
                dt = data[i]
                dt2 = data2[i]

            # Apply col() to the data points in the row
            for j in range(len(data[i])):
                if mem_usage: t = col(data[i][j], data2[i][j])
                else: t = col(dt[j], dt2[j])
                
                tmp[i][j] = t
                
                if visualize_report:
                    if t<0: unresolved+=1
                    elif t>1: nll+=1
                    elif t>0: fix+=1
        
        if visualize_report:
            print("unresolved",unresolved)
            print("still null",nll)
            print("fixed",fix)
    else:
        # Iterate through data
        for i in range(len(data)):
            if verbose: print("%d/%d"%(i, shape[0]))
            
            dt = data[i]
            dt2 = data2[i]

            # Apply fixit() to the data points in the row
            for j in range(len(data[i])): tmp[i][j] = fixit(dt[j], dt2[j])
    return tmp

def plot(data, mask=False):
    """
    Plot the height map using matplotlib
    
    Args:
        data (numpy.array): Data array
        mask (bool): Mask zeros in the data (hide them)
    """
    fig = plt.figure()
    
    ax = fig.add_subplot(111)
    ax.set_title('Height Map')
    
    if mask: data = np.ma.masked_where(x==0, data)
    
    plt.imshow(data)
    plt.gca().invert_yaxis()
    
    plt.show()