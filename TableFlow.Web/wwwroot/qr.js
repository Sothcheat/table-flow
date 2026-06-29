// Opens a dedicated print window with just the receipt — avoids printing the whole app page.
// data: { tableNumber, dateTime, lines: [{itemName, varientName, quantity, totalPrice}],
//         grandTotal, paymentMethod, amountReceived, change }
window.printReceipt = function (data) {
    const w = window.open('', '_blank', 'width=460,height=640');
    if (!w) return;

    var rows = data.lines.map(function (l) {
        var name = l.itemName + (l.varientName ? ' (' + l.varientName + ')' : '');
        return '<tr><td>' + name + '</td><td class="c">' + l.quantity +
               '</td><td class="r">$' + l.totalPrice.toFixed(2) + '</td></tr>';
    }).join('');

    var paymentRows = '';
    if (data.paymentMethod) {
        paymentRows += '<tr><td class="lbl">Payment</td><td class="val">' + data.paymentMethod + '</td></tr>';
        if (data.paymentMethod === 'Cash') {
            paymentRows += '<tr><td class="lbl">Received</td><td class="val">$' + data.amountReceived.toFixed(2) + '</td></tr>';
            paymentRows += '<tr class="change"><td class="lbl">Change</td><td class="val">$' + data.change.toFixed(2) + '</td></tr>';
        }
    }

    w.document.write(
        '<!DOCTYPE html><html><head><title>Receipt — Table ' + data.tableNumber + '</title>' +
        '<style>' +
        'body{font-family:sans-serif;font-size:13px;margin:0;padding:24px;color:#111;}' +
        'h1{font-size:20px;font-weight:800;text-align:center;margin:0 0 2px;}' +
        '.sub{text-align:center;color:#555;font-size:12px;margin-bottom:16px;}' +
        'hr{border:none;border-top:1px dashed #ccc;margin:12px 0;}' +
        'table{width:100%;border-collapse:collapse;}' +
        'th{font-size:11px;font-weight:700;text-transform:uppercase;color:#555;padding:4px 0;border-bottom:1px solid #ddd;}' +
        'td{padding:5px 0;vertical-align:top;}' +
        '.c{text-align:center;width:40px;}.r{text-align:right;width:70px;}' +
        '.total-row td{font-size:15px;font-weight:800;padding-top:10px;}' +
        '.pay-table{margin-top:10px;}.lbl{color:#555;}.val{text-align:right;font-weight:600;}' +
        '.change td{color:#15803d;font-weight:700;}' +
        '.footer{text-align:center;color:#888;font-size:11px;margin-top:16px;}' +
        '</style></head>' +
        '<body onload="window.focus();window.print();">' +
        '<h1>TABLEFLOW</h1>' +
        '<div class="sub">Table ' + data.tableNumber + '<br>' + data.dateTime + '</div>' +
        '<hr>' +
        '<table><thead><tr><th>Item</th><th class="c">Qty</th><th class="r">Price</th></tr></thead>' +
        '<tbody>' + rows + '</tbody>' +
        '<tfoot><tr class="total-row"><td colspan="2">Total</td><td class="r">$' + data.grandTotal.toFixed(2) + '</td></tr></tfoot>' +
        '</table>' +
        (paymentRows ? '<hr><table class="pay-table"><tbody>' + paymentRows + '</tbody></table>' : '') +
        '<hr><div class="footer">Thank you for dining with us!</div>' +
        '</body></html>'
    );
    w.document.close();
};

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
