window.addEventListener("DOMContentLoaded", () => {
    const $ = window.$;

    let worker = new Worker("/js/patcher/app_worker.js");

    let version_map = new Map();
    version_map.set("GZ2E01\0\0", "USA");
    version_map.set("GZ2P01\0\0", "EUR");
    version_map.set("GZ2J01\0\0", "JAP");
    version_map.set("RZDE01\0\0", "WUS");
    version_map.set("RZDP01\0\0", "WEU");
    version_map.set("RZDJ01\0\0", "WJP");
    version_map.set("RZDE01\0\x02", "WU2");

    let patching_status = (() => {
        let is_patching = false;
        return {
            get is_patching() { return is_patching; },
            set is_patching(value) {
                if (value) {
                    $("#patch_btn").attr("disabled", "disabled");
                    $("#patch_status").show();
                    $("#patch_error").hide();
                    $("#patch_progress").removeAttr("value");
                    $("#iso_in").attr("disabled", "disabled");
                } else {
                    $("#patch_status").hide();
                    $("#patch_error").hide();
                    $("#patch_progress").removeAttr("value");
                    $("#iso_in").removeAttr("disabled");
                    if ($("#iso_in")[0].files.length === 0) {
                        $("#patch_btn").attr("disabled", "disabled");
                    } else {
                        $("#patch_btn").removeAttr("disabled");
                    }
                }
                is_patching = value
            },
        }
    })();

    patching_status.is_patching = false;

    function base64ToBytes(base64) {
        const binString = atob(base64);
        return Uint8Array.from(binString, (m) => m.codePointAt(0));
    }

    $("#iso_in").on("change", (evt) => {
        let file = evt.target.files[0];
        if (file) {
            if (patching_status.is_patching) {
                $("#patch_btn").attr("disabled", "disabled");
            } else {
                $("#patch_btn").removeAttr("disabled");
            }
        } else {
            $("#patch_btn").removeAttr("disabled");
        }
    });

    let patch_name = null;

    $("#patch_btn").on("click", (evt) => {
        if ($("#iso_in")[0].files.length === 0) {
            $("#patch_error").text("Please select an ISO file.").show();
            return;
        }
        $("#patch_error").hide();
        $("#patch_status_text").text("Loading...");
        $("#patch_progress").removeAttr("value");

        patching_status.is_patching = true;

        // Read first 8 bytes of the iso to get the version
        let versionPromise = new Promise((resolve, reject) => {
            let reader = new FileReader();
            reader.addEventListener("load", (evt) => {
                let bytes = new Uint8Array(evt.target.result);
                let version = version_map.get(String.fromCharCode(...bytes.slice(0, 8)));
                if (!version) {
                    patching_status.is_patching = false;
                    console.error("Unknown version", bytes.slice(0, 8));
                    $("#patch_error").text("Unknown version.").show();
                    reject("Unknown version.");
                    return;
                }
                console.debug("Version", version);
                resolve(version);
            });
            reader.readAsArrayBuffer($("#iso_in")[0].files[0]);
            reader.addEventListener("error", (evt) => {
                patching_status.is_patching = false;
                console.error("Error reading version", evt);
                $("#patch_error").text("Failed to read version.").show();
                reject("Failed to read version.");
            });
        });

        versionPromise.then((version) => {
            let fcSettings = window.tpr.shared.genFcSettingsString(true, version);
            return window.tpr.shared.callCreateGci(fcSettings).then((data) => {
                console.info('success in response');
                console.info(data);
                $('#patch_error').hide();
                let {name, bytes} = data[0];
                patch_name = name.replace(/\.patch\s*$/g, '.iso');
                console.info(name, patch_name);
                let patchBytes = base64ToBytes(bytes);
                console.info(patchBytes);
                let patch = new Blob([patchBytes], { type: 'application/octet-stream' });
                let file = $("#iso_in")[0].files[0];
                worker.postMessage({ type: "run", file, patch });
            }).catch((error) => {
                patching_status.is_patching = false;
                console.log('error in response');
                console.log(error);
                $('#patch_error').text('Failed to get patch.').show();
            });
        }).catch((err) => {
            patching_status.is_patching = false;
            console.log('error in response');
            console.log(err);
            $('#patch_error').text('Failed to launch patching process: ' + err).show();
        });
    });

    function setupDownload(file, filename) {
        let a = document.createElement("a");
        a.style.display = "none";
        a.download = filename;
        let url = window.URL.createObjectURL(file);
        a.href = url;
        document.body.appendChild(a);
        a.click();
        console.debug("Download done. Cleaning...");
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    }

    async function downloadIso(filename) {
        let root = await navigator.storage.getDirectory();
        let fileHandle = await root.getFileHandle("out.iso");
        setupDownload(await fileHandle.getFile(), filename);
    }

    worker.addEventListener("message", (event) => {
        switch (event.data.type) {
            case "progress": {
                if (typeof event.data.progress !== "undefined") {
                    $("#patch_status_text").text(event.data.title);
                } else {
                    $("#patch_status_text").text("");
                }
                if (typeof event.data.progress == "number") {
                    $("#patch_progress").attr("value", event.data.progress);
                    $("#patch_progress_text").text(event.data.progress.toLocaleString(undefined, {maximumFractionDigits: 1, minimumFractionDigits: 1}) + "%");
                } else {
                    $("#patch_progress_text").text("");
                    $("#patch_progress").removeAttr("value");
                }
                if (typeof event.data.title !== "string" && typeof event.data.progress !== "number") {
                    $("#patch_status").hide();
                } else {
                    $("#patch_status").show();
                }
                break;
            }
            case "done": {
                patching_status.is_patching = false;
                if (patch_name === null) {
                    console.warn("No patch name set. Using", event.data.filename);
                    patch_name = event.data.filename;
                    console.debug("Done", event.data.filename);
                } else {
                    console.debug("Done", event.data.filename, "=>", patch_name);
                }
                $("#patch_status_text").text("Done");
                downloadIso(patch_name);
                $("#patch_status").hide();
                patch_name = null;
                break;
            }
            case "cancelled": {
                patching_status.is_patching = false;
                console.debug("Cancelled", event.data.msg);
                $("#patch_status_text").text("Cancelled");
                $("#patch_error").text(event.data.msg).show();
                break;
            }
        }
    });
});