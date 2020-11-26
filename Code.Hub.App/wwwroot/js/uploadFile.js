function getFileData(inputFile) {
    var fileToLoad = document.getElementById(inputFile).files[0];
    const temporaryFileReader = new FileReader();

    return new Promise((resolve, reject) => {
        temporaryFileReader.onerror = () => {
            temporaryFileReader.abort();
            reject(new DOMException("Problem parsing input file."));
        };
        temporaryFileReader.addEventListener("load", function () {
            //var data = {
            //    content: new Int8Array(temporaryFileReader.result).toString()
            //};
            //console.log(JSON.stringify(data));

            var data = {
                content: temporaryFileReader.result.split(',')[1]
            };

            resolve(data);
        }, false);
        //temporaryFileReader.readAsArrayBuffer(fileToLoad);
        temporaryFileReader.readAsDataURL(fileToLoad);

    });

};
