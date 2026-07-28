document.querySelectorAll("input[type=tel]").forEach(x=>x.addEventListener("input",()=>x.value=x.value.replace(/\D/g,"")));
