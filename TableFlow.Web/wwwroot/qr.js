// Opens a print-friendly window containing only the table QR and prints it.
window.printQrCode = function (base64, tableNumber) {
    const w = window.open('', '_blank');
    if (!w) return;
    w.document.write(
        '<html><head><title>Table ' + tableNumber + ' QR</title>' +
        '<style>body{font-family:sans-serif;text-align:center;padding:40px;margin:0;}' +
        'img{width:320px;height:320px;}h1{font-size:24px;margin:0 0 12px;}' +
        'p{color:#555;font-size:14px;margin-top:12px;}</style></head>' +
        '<body onload="window.focus();window.print();">' +
        '<h1>Table ' + tableNumber + '</h1>' +
        '<img src="data:image/png;base64,' + base64 + '" alt="QR Code" />' +
        '<p>Scan to view the menu &amp; place your order</p>' +
        '</body></html>'
    );
    w.document.close();
};
