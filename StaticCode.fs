module StaticCode

let pyodideAndTurfLoader = 
    $"""
    let cr = interactive.configureRequire({{
        paths: {{
            pyodide: "https://cdn.jsdelivr.net/pyodide/v0.20.0/full/pyodide",
            turfpath: "https://unpkg.com/@turf/turf@6/turf.min"
        }}
    }});
    cr(["pyodide", "turfpath"], function (pyodide, turf) {{
    
        loadPyodide().then(pyodide => {{ 
            window.pyodide = pyodide; 
            window.python = pyodide.runPython;
            window.pythonAsync = pyodide.runPythonAsync;
            //import math here
            pyodide.runPython("import math");
            console.log("pyscript is running, here goes the proof: " + pyodide.runPython("1 + 3"));
        }});

        window.turf = turf;
        var from = turf.point([-75.343, 39.984]);
        var to = turf.point([-75.534, 39.123]);
        var options = {{units: 'kilometers'}};
        var distance = turf.distance(from, to, options);

        console.log("Proof turfjs works:" + distance);
    
    }});
    """