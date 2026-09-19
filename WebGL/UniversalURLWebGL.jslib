var UniversalURLPlugin = {
    OpenNewTab : function(url)
    {
        // Modern Unity WebGL: Use UTF8ToString (Unity 2021.2+)
        // This is the recommended way to convert C# strings to JavaScript strings
        if (typeof UTF8ToString !== 'undefined') {
            url = UTF8ToString(url);
        } else {
            // Fallback for older Unity versions (should not be needed in modern Unity)
            console.warn('OpenNewTab: UTF8ToString not available, trying alternative methods');
            try {
                // Try Module.UTF8ToString for older Unity versions
                if (typeof Module !== 'undefined' && Module.UTF8ToString) {
                    url = Module.UTF8ToString(url);
                } else {
                    // Last resort: assume url is already a string (should not happen)
                    console.warn('OpenNewTab: Could not convert URL, using as-is');
                }
            } catch(e) {
                console.error('OpenNewTab: Error converting URL:', e);
            }
        }
        window.open(url, '_blank');
    },
    
    OpenURLWebGL : function(url)
    {
        // Modern Unity WebGL: Use UTF8ToString (Unity 2021.2+)
        // This is the recommended way to convert C# strings to JavaScript strings
        if (typeof UTF8ToString !== 'undefined') {
            url = UTF8ToString(url);
        } else {
            try {
                if (typeof Module !== 'undefined' && Module.UTF8ToString) {
                    url = Module.UTF8ToString(url);
                }
            } catch(e) {
                console.error('OpenURLWebGL: Error converting URL:', e);
            }
        }
        window.open(url, '_blank');
    }
};
mergeInto(LibraryManager.library, UniversalURLPlugin);
