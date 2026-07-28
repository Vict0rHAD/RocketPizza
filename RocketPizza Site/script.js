document.addEventListener("DOMContentLoaded", () => {
    const authModal = document.querySelector(".auth-modal");
    const abrirModal = document.getElementById("abrirModal");
    const fecharAuth = document.querySelector(".auth-close");
    const loginBox = document.getElementById("loginBox");
    const cadastroBox = document.getElementById("cadastroBox");
    const irCadastro = document.getElementById("irCadastro");
    const voltarLogin = document.getElementById("voltarLogin");
    const formLogin = document.getElementById("formLogin");
    const formCadastro = document.getElementById("formCadastro");
    const pedidoPage = document.querySelector(".pedidos-page");

    const storage = {
        users: "rocketPizzaUsuarios",
        current: "rocketPizzaUsuarioAtual",
        logged: "rocketPizzaLogado",
        destination: "rocketPizzaDestino"
    };

    const isLogged = () => localStorage.getItem(storage.logged) === "sim" && !!localStorage.getItem(storage.current);
    const normalizeEmail = email => (email || "").trim().toLowerCase();
    const getUsers = () => JSON.parse(localStorage.getItem(storage.users) || "{}");
    const saveUsers = users => localStorage.setItem(storage.users, JSON.stringify(users));
    const getCurrentEmail = () => normalizeEmail(localStorage.getItem(storage.current));
    const getCurrentUser = () => {
        const email = getCurrentEmail();
        return email ? getUsers()[email] || null : null;
    };
    const setPendingOrder = () => localStorage.setItem(storage.destination, "pedidos.html");
    const clearPendingOrder = () => localStorage.removeItem(storage.destination);

    function saveUserProfile(user) {
        if (!user || !user.email) return null;
        const users = getUsers();
        const email = normalizeEmail(user.email);
        users[email] = {
            nome: user.nome ?? users[email]?.nome ?? "Cliente Rocket",
            email,
            telefone: user.telefone ?? users[email]?.telefone ?? "",
            endereco: user.endereco ?? users[email]?.endereco ?? "",
            numero: user.numero ?? users[email]?.numero ?? "",
            senha: user.senha ?? users[email]?.senha ?? "",
            pedidos: user.pedidos ?? users[email]?.pedidos ?? []
        };
        saveUsers(users);
        localStorage.setItem(storage.current, email);
        return users[email];
    }

    function setLogged(user) {
        const savedUser = saveUserProfile(user);
        if (!savedUser) return;
        localStorage.setItem(storage.logged, "sim");
        localStorage.setItem(storage.current, savedUser.email);
    }

    function userName(user = getCurrentUser()) {
        if (!user) return "Minha conta";
        return (user.nome || user.email || "Minha conta").split(" ")[0];
    }

    function renderPedidosNavLink() {
        const navList = document.querySelector(".nav-list");
        if (!navList || !isLogged() || navList.querySelector('a[href="pedidos.html"]')) return;

        const unidadesLink = navList.querySelector('a[href="unidades.html"]')?.closest("li");
        const item = document.createElement("li");
        item.innerHTML = `<a href="pedidos.html">Pedidos</a>`;
        navList.insertBefore(item, unidadesLink || null);
    }

    function openAuth(redirectToOrder = false) {
        if (!authModal) return;
        if (redirectToOrder) setPendingOrder();
        authModal.classList.add("active");
        if (loginBox && cadastroBox) {
            loginBox.style.display = "block";
            cadastroBox.style.display = "none";
        }
    }

    function closeAuth() {
        if (authModal) authModal.classList.remove("active");
    }

    function afterAuthSuccess(user) {
        setLogged(user);
        const destination = localStorage.getItem(storage.destination);
        clearPendingOrder();
        window.location.href = destination || "pedidos.html";
    }

    function renderAccountMenu() {
        if (!abrirModal) return;
        const existingWrapper = abrirModal.closest(".account-menu");

        if (!isLogged()) {
            abrirModal.textContent = "Login";
            abrirModal.onclick = () => openAuth(false);
            existingWrapper?.classList.remove("logged");
            return;
        }

        abrirModal.textContent = `👤 ${userName()}`;
        abrirModal.type = "button";
        abrirModal.onclick = null;

        const wrapper = existingWrapper || document.createElement("div");
        if (!existingWrapper) {
            wrapper.className = "account-menu";
            abrirModal.parentNode.insertBefore(wrapper, abrirModal);
            wrapper.appendChild(abrirModal);
        }

        wrapper.classList.add("logged");
        if (!wrapper.querySelector(".account-dropdown")) {
            const dropdown = document.createElement("div");
            dropdown.className = "account-dropdown";
            dropdown.innerHTML = `
                <a href="perfil.html">Perfil</a>
                <a href="meus-pedidos.html">Meus pedidos</a>
                <button type="button" id="logoutConta">Sair</button>
            `;
            wrapper.appendChild(dropdown);
        }

        wrapper.querySelector("#logoutConta")?.addEventListener("click", () => {
            if (!confirm("Você realmente quer sair da conta?")) return;
            localStorage.removeItem(storage.logged);
            localStorage.removeItem(storage.current);
            localStorage.removeItem(storage.destination);
            window.location.href = "index.html";
        });
    }

    if (abrirModal) abrirModal.addEventListener("click", () => {
        if (!isLogged()) openAuth(false);
    });

    if (fecharAuth) fecharAuth.addEventListener("click", closeAuth);
    if (authModal) {
        authModal.addEventListener("click", event => {
            if (event.target === authModal) closeAuth();
        });
    }

    document.querySelectorAll('a[href="pedidos.html"]').forEach(link => {
        link.addEventListener("click", event => {
            if (isLogged()) return;
            event.preventDefault();
            openAuth(true);
        });
    });

    if (irCadastro && loginBox && cadastroBox) {
        irCadastro.addEventListener("click", () => {
            loginBox.style.display = "none";
            cadastroBox.style.display = "block";
        });
    }

    if (voltarLogin && loginBox && cadastroBox) {
        voltarLogin.addEventListener("click", () => {
            cadastroBox.style.display = "none";
            loginBox.style.display = "block";
        });
    }

    if (formLogin) {
        formLogin.addEventListener("submit", event => {
            event.preventDefault();
            const email = normalizeEmail(formLogin.querySelector('input[type="email"]')?.value);
            const senha = formLogin.querySelector('input[type="password"]')?.value || "";
            const users = getUsers();
            const user = users[email];
            if (!user || user.senha !== senha) {
                alert("E-mail ou senha inválidos. Cadastre-se antes de entrar.");
                return;
            }
            afterAuthSuccess(user);
        });
    }

    if (formCadastro) {
        formCadastro.addEventListener("submit", event => {
            event.preventDefault();
            const campos = formCadastro.querySelectorAll("input");
            const senha = document.getElementById("senhaCadastro")?.value || "";
            const confirmar = document.getElementById("confirmarSenhaCadastro")?.value || "";

            if (senha !== confirmar) {
                alert("As senhas não coincidem.");
                return;
            }

            const email = normalizeEmail(campos[1]?.value);
            if (getUsers()[email]) {
                alert("Já existe uma conta cadastrada com este e-mail.");
                return;
            }

            afterAuthSuccess({
                nome: campos[0]?.value.trim(),
                email,
                telefone: campos[2]?.value.trim(),
                idade: Number(campos[3]?.value),
                endereco: "",
                numero: "",
                senha
            });
        });
    }

    if (pedidoPage && !isLogged()) {
        pedidoPage.classList.add("pedido-bloqueado");
        setPendingOrder();
        openAuth(true);
    }

    renderPedidosNavLink();
    renderAccountMenu();
    iniciarPerfil({ isLogged, openAuth, getCurrentUser, saveUserProfile, renderAccountMenu });
    iniciarHistorico({ isLogged, openAuth, getCurrentUser });
    iniciarCardapio();
    iniciarPedidos({ getCurrentUser, saveUserProfile });
});

/* Camada profissional e centralizada de validação Rocket Pizza. */
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("#formCadastro").forEach(form => {
        if (form.querySelector("[name='idade']")) return;
        const idade = document.createElement("input");
        idade.type = "number"; idade.name = "idade"; idade.placeholder = "Idade";
        idade.min = "13"; idade.max = "120"; idade.step = "1"; idade.required = true;
        idade.dataset.apenasNumeros = "true"; idade.inputMode = "numeric";
        form.querySelector("#senhaCadastro")?.insertAdjacentElement("beforebegin", idade);
    });
    const regrasSenha = [
        ["8 caracteres", value => value.length >= 8],
        ["letra maiúscula", value => /[A-ZÁÉÍÓÚÂÊÔÃÕÇ]/.test(value)],
        ["letra minúscula", value => /[a-záéíóúâêôãõç]/.test(value)],
        ["número", value => /\d/.test(value)],
        ["caractere especial", value => /[^A-Za-zÀ-ÿ0-9\s]/.test(value)],
        ["sem espaços", value => !/\s/.test(value)]
    ];
    const textoSeguro = /^[A-Za-zÀ-ÿ][A-Za-zÀ-ÿ' -]*$/;
    const emailSeguro = /^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$/;
    const telefoneSeguro = /^\(?\d{2}\)?\s?9?\d{4}-?\d{4}$/;

    function erro(campo, mensagem) {
        campo.setCustomValidity(mensagem || "");
        campo.classList.toggle("campo-invalido", !!mensagem);
    }
    function validarCampo(campo) {
        const valor = campo.value;
        erro(campo, "");
        if (valor && /^\s/.test(valor)) return erro(campo, "O campo não pode começar com espaço."), false;
        if (campo.type === "email" && valor && !emailSeguro.test(valor)) return erro(campo, "Informe um e-mail válido, como nome@dominio.com."), false;
        if (campo.type === "tel" && valor && !telefoneSeguro.test(valor)) return erro(campo, "Informe um telefone brasileiro válido com DDD."), false;
        if ((campo.id?.toLowerCase().includes("nome") || campo.placeholder?.toLowerCase().includes("nome")) && valor && !textoSeguro.test(valor)) return erro(campo, "Use apenas letras, espaços, hífen ou apóstrofo."), false;
        if ((campo.id === "perfilNumero" || campo.dataset.apenasNumeros === "true") && valor && !/^\d+$/.test(valor)) return erro(campo, "Use apenas números."), false;
        if (campo.tagName === "TEXTAREA" && valor.length > 500) return erro(campo, "O texto deve ter no máximo 500 caracteres."), false;
        return true;
    }

    document.querySelectorAll("input, textarea").forEach(campo => {
        campo.autocomplete = campo.type === "password" ? "current-password" : campo.autocomplete;
        if (campo.type === "email") campo.maxLength = 254;
        if (campo.type === "tel") { campo.maxLength = 15; campo.inputMode = "tel"; }
        if (campo.type === "text") campo.maxLength = campo.maxLength > 0 ? campo.maxLength : 120;
        if (campo.tagName === "TEXTAREA") campo.maxLength = 500;
        campo.addEventListener("input", () => validarCampo(campo));
        campo.addEventListener("blur", () => { campo.value = campo.value.trim(); validarCampo(campo); });
    });

    document.querySelectorAll("#senhaCadastro").forEach(senha => {
        senha.minLength = 8; senha.maxLength = 72; senha.autocomplete = "new-password";
        const painel = document.createElement("ul");
        painel.className = "senha-requisitos";
        painel.setAttribute("aria-live", "polite");
        regrasSenha.forEach(([rotulo]) => painel.insertAdjacentHTML("beforeend", `<li>○ ${rotulo}</li>`));
        senha.insertAdjacentElement("afterend", painel);
        const atualizar = () => {
            [...painel.children].forEach((li, i) => { const ok = regrasSenha[i][1](senha.value); li.className = ok ? "ok" : ""; li.textContent = `${ok ? "✓" : "○"} ${regrasSenha[i][0]}`; });
            const valida = regrasSenha.every(([, teste]) => teste(senha.value));
            erro(senha, senha.value && !valida ? "Complete todos os requisitos da senha." : "");
        };
        senha.addEventListener("input", atualizar); atualizar();
    });

    document.querySelectorAll("form").forEach(form => form.addEventListener("submit", event => {
        const campos = [...form.querySelectorAll("input, textarea")];
        const senha = form.querySelector("#senhaCadastro");
        const confirmar = form.querySelector("#confirmarSenhaCadastro");
        campos.forEach(validarCampo);
        if (senha && !regrasSenha.every(([, teste]) => teste(senha.value))) erro(senha, "Complete todos os requisitos da senha.");
        if (senha && confirmar && senha.value !== confirmar.value) erro(confirmar, "As senhas não coincidem."); else if (confirmar) erro(confirmar, "");
        if (!form.checkValidity()) { event.preventDefault(); event.stopImmediatePropagation(); form.reportValidity(); }
    }, true));

    const contato = document.getElementById("formContato");
    contato?.addEventListener("submit", event => {
        event.preventDefault();
        if (!contato.checkValidity()) return contato.reportValidity();
        alert("Mensagem validada e enviada com sucesso!"); contato.reset();
    });
});

function iniciarPerfil(auth) {
    const form = document.getElementById("perfilForm");
    if (!form) return;

    if (!auth.isLogged()) {
        document.querySelector(".perfil-page")?.classList.add("pedido-bloqueado");
        auth.openAuth(false);
        return;
    }

    const user = auth.getCurrentUser();
    document.getElementById("perfilNome").value = user?.nome || "";
    document.getElementById("perfilTelefone").value = user?.telefone || "";
    document.getElementById("perfilEndereco").value = user?.endereco || "";
    document.getElementById("perfilNumero").value = user?.numero || "";
    document.getElementById("perfilEmail").value = user?.email || "";

    form.addEventListener("submit", event => {
        event.preventDefault();
        auth.saveUserProfile({
            nome: document.getElementById("perfilNome").value.trim(),
            telefone: document.getElementById("perfilTelefone").value.trim(),
            endereco: document.getElementById("perfilEndereco").value.trim(),
            numero: document.getElementById("perfilNumero").value.trim(),
            email: document.getElementById("perfilEmail").value.trim(),
            senha: user?.senha || "",
            pedidos: user?.pedidos || []
        });
        auth.renderAccountMenu();
        document.getElementById("perfilMensagem").textContent = "Dados atualizados com sucesso.";
    });
}

function iniciarHistorico(auth) {
    const lista = document.getElementById("pedidosHistorico");
    if (!lista) return;

    if (!auth.isLogged()) {
        document.querySelector(".historico-page")?.classList.add("pedido-bloqueado");
        auth.openAuth(false);
        return;
    }

    const user = auth.getCurrentUser();
    const pedidos = user?.pedidos || [];

    if (!pedidos.length) {
        lista.innerHTML = `<div class="historico-vazio">Você ainda não fez nenhum pedido.</div>`;
        return;
    }

    lista.innerHTML = pedidos.map(pedido => `
        <article class="historico-item">
            <div>
                <span class="historico-numero">Pedido #${pedido.numero}</span>
                <h2>${pedido.sabores}</h2>
                <p>${pedido.data} - ${pedido.total}</p>
            </div>
            <button class="btn ver-comprovante" type="button" data-pedido="${pedido.numero}">Ver comprovante</button>
        </article>
    `).join("");

    lista.querySelectorAll(".ver-comprovante").forEach(botao => {
        botao.addEventListener("click", () => {
            const pedido = pedidos.find(item => String(item.numero) === String(botao.dataset.pedido));
            if (pedido) abrirComprovante(pedido.comprovante);
        });
    });
}

function iniciarCardapio() {
    const cardapioLinks = document.querySelectorAll(".cardapio-link");

    cardapioLinks.forEach(link => {
        link.addEventListener("click", event => {
            event.preventDefault();
            const target = document.querySelector(link.getAttribute("href"));
            if (target) target.scrollIntoView({ behavior: "smooth", block: "start" });

            cardapioLinks.forEach(item => item.classList.remove("active"));
            link.classList.add("active");
        });
    });

    if (cardapioLinks.length) {
        window.addEventListener("scroll", () => {
            let current = "";
            document.querySelectorAll(".cardapio-section").forEach(section => {
                if (window.scrollY >= section.offsetTop - 160) current = section.id;
            });

            cardapioLinks.forEach(link => {
                link.classList.toggle("active", link.getAttribute("href") === `#${current}`);
            });
        });
    }

    const modalPizza = document.getElementById("modalPizza");
    const modalImg = document.getElementById("modalImg");
    const modalTitulo = document.getElementById("modalTitulo");
    const modalDesc = document.getElementById("modalDesc");
    const fechar = document.querySelector(".fechar");

    if (!modalPizza || !modalImg || !modalTitulo || !modalDesc || !fechar) return;

    document.querySelectorAll(".menu-item").forEach(item => {
        item.addEventListener("click", () => {
            modalTitulo.textContent = item.dataset.nome || "";
            modalImg.src = item.dataset.img || "";
            modalDesc.textContent = item.dataset.desc || "";
            modalPizza.style.display = "flex";
        });
    });

    fechar.addEventListener("click", () => modalPizza.style.display = "none");
    modalPizza.addEventListener("click", event => {
        if (event.target === modalPizza) modalPizza.style.display = "none";
    });
}

function iniciarPedidos(auth) {
    const saborUm = document.getElementById("saborUm");
    const saborDois = document.getElementById("saborDois");
    const tipoPizza = document.getElementById("tipoPizza");
    const tamanhoPizza = document.getElementById("tamanhoPizza");
    const entradaPedido = document.getElementById("entradaPedido");
    const quantidadeEntrada = document.getElementById("quantidadeEntrada");
    const bebidaPedido = document.getElementById("bebidaPedido");
    const quantidadeBebida = document.getElementById("quantidadeBebida");
    const adicionarCarrinho = document.getElementById("adicionarCarrinho");

    if (!saborUm || !saborDois || !tipoPizza || !tamanhoPizza || !entradaPedido || !quantidadeEntrada || !bebidaPedido || !quantidadeBebida || !adicionarCarrinho) {
        return;
    }

    const user = auth.getCurrentUser?.();
    if (user) {
        document.getElementById("nomeCliente").value = user.nome || "";
        document.getElementById("telefoneCliente").value = user.telefone || "";
        document.getElementById("enderecoCliente").value = [user.endereco, user.numero].filter(Boolean).join(", ");
    }

    const sabores = [
        { nome: "Combo Família", media: 89.90, grande: 89.90, combo: true },
        { nome: "Marguerita", media: 64.90, grande: 79.90 },
        { nome: "Calabresa", media: 64.90, grande: 79.90 },
        { nome: "Frango com Catupiry", media: 64.90, grande: 79.90 },
        { nome: "Portuguesa", media: 69.90, grande: 84.90 },
        { nome: "Pepperoni", media: 72.90, grande: 89.90 },
        { nome: "Quatro Queijos", media: 72.90, grande: 89.90 },
        { nome: "Mozzarella", media: 56.90, grande: 69.90 },
        { nome: "Napolitana", media: 64.90, grande: 79.90 },
        { nome: "Atum", media: 72.90, grande: 89.90 },
        { nome: "Rocket Supreme", media: 84.90, grande: 99.90 },
        { nome: "Galáxia", media: 79.90, grande: 94.90 },
        { nome: "Meteoro", media: 74.90, grande: 89.90 },
        { nome: "Nebulosa", media: 84.90, grande: 99.90 },
        { nome: "Constelação", media: 79.90, grande: 94.90 },
        { nome: "Órbita", media: 74.90, grande: 89.90 },
        { nome: "Chocolate", media: 59.90, grande: 72.90 },
        { nome: "Banana com Canela", media: 56.90, grande: 69.90 },
        { nome: "Nutella", media: 64.90, grande: 79.90 }
    ];
    const entradas = [
        { nome: "Bruschetta Clássica", preco: 29.90 },
        { nome: "Bolinha de Queijo", preco: 34.90 },
        { nome: "Focaccia Rocket", preco: 24.90 },
        { nome: "Palitos de Mozzarella", preco: 32.90 },
        { nome: "Anéis de Cebola", preco: 27.90 },
        { nome: "Salada Caesar", preco: 26.90 }
    ];
    const bebidas = [
        { nome: "Refrigerante Lata", preco: 8.90 },
        { nome: "Refrigerante 2L", preco: 16.90 },
        { nome: "Suco Natural", preco: 14.90 },
        { nome: "Água Mineral", preco: 6.90 },
        { nome: "Cerveja Artesanal", preco: 19.90 },
        { nome: "Chopp", preco: 14.90 }
    ];
    const entrega = 5;
    const moeda = valor => valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });
    const pagamentoTexto = { pix: "Pix", cartao: "Cartão de crédito", dinheiro: "Dinheiro na entrega" };
    let cartaoPreenchido = false;
    let dinheiroPreenchido = false;
    let carrinho = [];

    sabores.forEach((sabor, index) => {
        saborUm.add(new Option(sabor.nome, index));
    });

    function popularSaboresDois() {
        saborDois.innerHTML = "";
        sabores.forEach((sabor, index) => {
            if (!sabor.combo) saborDois.add(new Option(sabor.nome, index));
        });
        saborDois.value = "1";
    }

    popularSaboresDois();

    popularAdicionais(entradaPedido, entradas, moeda);
    popularAdicionais(bebidaPedido, bebidas, moeda);

    function adicionalSelecionado(select, itens) {
        return select.value === "" ? null : itens[Number(select.value)];
    }

    function quantidade(input) {
        return Math.max(0, Number(input.value) || 0);
    }

    function calcularAdicional(select, input, itens) {
        const item = adicionalSelecionado(select, itens);
        const qtd = item ? quantidade(input) : 0;
        return { item, quantidade: qtd, total: item ? item.preco * qtd : 0 };
    }

    function textoAdicional(adicional) {
        if (!adicional.item || adicional.quantidade === 0) return "Não incluir";
        return `${adicional.quantidade}x ${adicional.item.nome} (${moeda(adicional.total)})`;
    }

    function obterResumo() {
        const primeiro = sabores[Number(saborUm.value)];
        const combo = !!primeiro.combo;
        const tamanho = combo ? "grande" : tamanhoPizza.value;
        const segundo = sabores[Number(saborDois.value)];
        const doisSabores = !combo && tipoPizza.value === "2";
        const pizza = combo ? primeiro.grande : (doisSabores ? Math.max(primeiro[tamanho], segundo[tamanho]) : primeiro[tamanho]);
        const entrada = calcularAdicional(entradaPedido, quantidadeEntrada, entradas);
        const bebida = calcularAdicional(bebidaPedido, quantidadeBebida, bebidas);
        const pagamento = document.querySelector("input[name='pagamento']:checked").value;
        const desconto = pagamento === "pix" ? pizza * 0.05 : 0;
        const total = pizza + entrada.total + bebida.total + entrega - desconto;

        return { tamanho, primeiro, segundo, doisSabores, combo, pizza, entrada, bebida, pagamento, desconto, total };
    }

    function atualizarResumo() {
        const resumo = obterResumo();
        const saborDoisGrupo = document.getElementById("saborDoisGrupo");
        const saborDoisLabel = document.getElementById("saborDoisLabel");
        const saboresInfo = document.getElementById("saboresInfo");
        const bebidaLabel = document.getElementById("bebidaLabel");
        const comboBebidaAviso = document.getElementById("comboBebidaAviso");

        if (resumo.combo) {
            tamanhoPizza.value = "grande";
            tipoPizza.value = "1";
            tamanhoPizza.disabled = true;
            tipoPizza.disabled = true;
        } else {
            tamanhoPizza.disabled = false;
            tipoPizza.disabled = false;
        }

        saborDoisGrupo.style.display = resumo.combo || resumo.doisSabores ? "block" : "none";
        saborDois.required = resumo.combo || resumo.doisSabores;
        if (saborDoisLabel) saborDoisLabel.textContent = resumo.combo ? "Sabor da pizza do combo" : "Segundo sabor";
        if (saboresInfo) {
            saboresInfo.textContent = resumo.combo
                ? "O Combo Família já é tamanho grande e leva uma pizza de sabor único."
                : "Para pizza de 2 sabores, o valor da pizza segue o sabor de maior preço.";
        }
        if (bebidaLabel) bebidaLabel.textContent = resumo.combo ? "Bebida adicional" : "Bebida";
        comboBebidaAviso?.classList.toggle("active", resumo.combo);

        document.getElementById("resumoSabores").textContent = resumo.combo
            ? `${resumo.primeiro.nome} - ${resumo.segundo.nome}`
            : resumo.doisSabores
            ? `${resumo.primeiro.nome} / ${resumo.segundo.nome}`
            : resumo.primeiro.nome;
        document.getElementById("resumoTamanho").textContent = resumo.tamanho === "media" ? "Média" : "Grande";
        document.getElementById("resumoPizza").textContent = moeda(resumo.pizza);
        document.getElementById("resumoEntrada").textContent = textoAdicional(resumo.entrada);
        document.getElementById("resumoBebida").textContent = resumo.combo
            ? `Inclui refrigerante grande${resumo.bebida.item && resumo.bebida.quantidade > 0 ? ` + ${textoAdicional(resumo.bebida)}` : ""}`
            : textoAdicional(resumo.bebida);
        document.getElementById("resumoEntrega").textContent = moeda(entrega);
        document.getElementById("resumoDesconto").textContent = resumo.desconto > 0 ? `-${moeda(resumo.desconto)}` : moeda(0);
        document.getElementById("resumoPagamento").textContent = pagamentoTexto[resumo.pagamento];
        document.getElementById("resumoTotal").textContent = moeda(resumo.total);
    }

    function abrirPedidoModal(id) {
        document.getElementById(id)?.classList.add("active");
    }

    function fecharPedidoModal(id) {
        document.getElementById(id)?.classList.remove("active");
    }

    function validarCartao() {
        return ["cartaoNome", "cartaoNumero", "cartaoValidade", "cartaoCvv"]
            .every(id => document.getElementById(id).value.trim().length > 0);
    }

    function validarDinheiro(totalMinimo = obterResumo().total) {
        const precisaTroco = document.querySelector("input[name='precisaTroco']:checked").value === "sim";
        if (!precisaTroco) return true;
        return Number(document.getElementById("valorNota").value) >= totalMinimo;
    }

    function montarComprovante(itens, totais) {
        const numeroPedido = Math.floor(100000 + Math.random() * 900000);
        const nome = document.getElementById("nomeCliente").value.trim();
        const telefone = document.getElementById("telefoneCliente").value.trim();
        const endereco = document.getElementById("enderecoCliente").value.trim();
        const pagamento = document.querySelector("input[name='pagamento']:checked").value;
        const saboresTexto = itens.map((item, index) => `${index + 1}. ${item.sabores} (${item.tamanho})`).join("<br>");
        const entradasTexto = itens.map((item, index) => `${index + 1}. ${item.entrada}`).join("<br>");
        const bebidasTexto = itens.map((item, index) => `${index + 1}. ${item.bebida}`).join("<br>");
        const observacoesTexto = itens
            .map((item, index) => `${index + 1}. ${item.observacoes}`)
            .join("<br>");
        const dinheiroInfo = pagamento === "dinheiro"
            ? `${document.querySelector("input[name='precisaTroco']:checked").value === "sim" ? moeda(Number(document.getElementById("valorNota").value) - totais.total) : "não precisa"}`
            : "";

        return {
            numero: numeroPedido,
            data: new Date().toLocaleString("pt-BR"),
            total: moeda(totais.total),
            sabores: itens.map(item => item.sabores).join(" + "),
            comprovante: {
                numero: numeroPedido,
                total: moeda(totais.total),
                nome,
                telefone,
                endereco,
                sabores: saboresTexto,
                entrada: entradasTexto,
                bebida: bebidasTexto,
                entrega: moeda(entrega),
                desconto: totais.desconto > 0 ? `-${moeda(totais.desconto)}` : moeda(0),
                pagamento: pagamentoTexto[pagamento],
                troco: dinheiroInfo,
                observacoes: observacoesTexto
            }
        };
    }

    function criarItemCarrinho(resumo) {
        const saboresTexto = resumo.combo
            ? `${resumo.primeiro.nome} - ${resumo.segundo.nome}`
            : resumo.doisSabores ? `${resumo.primeiro.nome} / ${resumo.segundo.nome}` : resumo.primeiro.nome;

        return {
            sabores: saboresTexto,
            tamanho: resumo.tamanho === "media" ? "Média" : "Grande",
            pizza: resumo.pizza,
            entrada: textoAdicional(resumo.entrada),
            entradaTotal: resumo.entrada.total,
            bebida: resumo.combo
                ? `Inclui refrigerante grande${resumo.bebida.item && resumo.bebida.quantidade > 0 ? ` + ${textoAdicional(resumo.bebida)}` : ""}`
                : textoAdicional(resumo.bebida),
            bebidaTotal: resumo.bebida.total,
            observacoes: document.getElementById("observacoes").value.trim() || "Nenhuma"
        };
    }

    function obterTotaisCarrinho() {
        const subtotal = carrinho.reduce((total, item) => total + item.pizza + item.entradaTotal + item.bebidaTotal, 0);
        const totalPizzas = carrinho.reduce((total, item) => total + item.pizza, 0);
        const pagamento = document.querySelector("input[name='pagamento']:checked").value;
        const desconto = pagamento === "pix" ? totalPizzas * 0.05 : 0;
        return { subtotal, desconto, total: subtotal + entrega - desconto };
    }

    function garantirCarrinhoUI() {
        let botao = document.getElementById("carrinhoFlutuante");
        let modal = document.getElementById("modalCarrinho");

        if (!botao) {
            botao = document.createElement("button");
            botao.id = "carrinhoFlutuante";
            botao.className = "carrinho-flutuante";
            botao.type = "button";
            botao.setAttribute("aria-label", "Abrir carrinho");
            botao.innerHTML = `<span aria-hidden="true">🛒</span><strong id="carrinhoQuantidade">0</strong>`;
            document.body.appendChild(botao);
        }

        if (!modal) {
            modal = document.createElement("div");
            modal.id = "modalCarrinho";
            modal.className = "pedido-modal carrinho-modal";
            modal.innerHTML = `
                <div class="pedido-modal-box carrinho-modal-box">
                    <button class="pedido-modal-close" id="fecharCarrinho" type="button" aria-label="Fechar carrinho">&times;</button>
                    <h2>Seu carrinho</h2>
                    <div id="carrinhoItens" class="carrinho-itens"></div>
                    <div class="carrinho-totais" id="carrinhoTotais"></div>
                    <button class="finalizar-btn" id="finalizarCarrinho" type="button">Finalizar pedido</button>
                </div>
            `;
            document.body.appendChild(modal);

            botao.addEventListener("click", () => {
                renderizarCarrinho();
                modal.classList.add("active");
            });
            modal.addEventListener("click", event => {
                if (event.target === modal) modal.classList.remove("active");
            });
            modal.querySelector("#fecharCarrinho").addEventListener("click", () => modal.classList.remove("active"));
            modal.querySelector("#finalizarCarrinho").addEventListener("click", finalizarCarrinho);
        }

        return { botao, modal };
    }

    function renderizarCarrinho() {
        const { botao, modal } = garantirCarrinhoUI();
        botao.classList.toggle("active", carrinho.length > 0);
        botao.querySelector("#carrinhoQuantidade").textContent = carrinho.length;

        const itens = modal.querySelector("#carrinhoItens");
        const totaisBox = modal.querySelector("#carrinhoTotais");
        itens.innerHTML = carrinho.map((item, index) => `
            <article class="carrinho-item">
                <div>
                    <span class="carrinho-item-numero">Pizza ${index + 1}</span>
                    <h3>${item.sabores}</h3>
                    <p>${item.tamanho} • ${moeda(item.pizza)}</p>
                    ${item.entrada !== "Não incluir" ? `<p>Entrada: ${item.entrada}</p>` : ""}
                    ${item.bebida !== "Não incluir" ? `<p>Bebida: ${item.bebida}</p>` : ""}
                    ${item.observacoes !== "Nenhuma" ? `<p>Obs.: ${item.observacoes}</p>` : ""}
                </div>
                <button class="carrinho-remover" type="button" data-indice="${index}" aria-label="Remover pizza ${index + 1}">Remover</button>
            </article>
        `).join("");

        const totais = obterTotaisCarrinho();
        totaisBox.innerHTML = `
            <div><span>Subtotal</span><strong>${moeda(totais.subtotal)}</strong></div>
            <div><span>Taxa de entrega</span><strong>${moeda(entrega)}</strong></div>
            <div><span>Desconto Pix</span><strong>-${moeda(totais.desconto)}</strong></div>
            <div class="carrinho-total"><span>Total</span><strong>${moeda(totais.total)}</strong></div>
        `;

        itens.querySelectorAll(".carrinho-remover").forEach(botaoRemover => {
            botaoRemover.addEventListener("click", () => {
                carrinho.splice(Number(botaoRemover.dataset.indice), 1);
                renderizarCarrinho();
                if (carrinho.length === 0) modal.classList.remove("active");
            });
        });
    }

    function limparMontagemPizza() {
        tamanhoPizza.value = "media";
        tipoPizza.value = "1";
        saborUm.value = "1";
        popularSaboresDois();
        entradaPedido.value = "";
        quantidadeEntrada.value = "0";
        bebidaPedido.value = "";
        quantidadeBebida.value = "0";
        document.getElementById("observacoes").value = "";
        atualizarResumo();
    }

    function finalizarCarrinho() {
        if (carrinho.length === 0) return;
        const camposCliente = ["nomeCliente", "telefoneCliente", "enderecoCliente"]
            .map(id => document.getElementById(id));
        if (camposCliente.some(campo => !campo.reportValidity())) return;

        const pagamento = document.querySelector("input[name='pagamento']:checked").value;
        const totais = obterTotaisCarrinho();

        if (pagamento === "cartao" && !cartaoPreenchido) {
            abrirPedidoModal("modalCartao");
            alert("Preencha os dados do cartão para finalizar.");
            return;
        }
        if (pagamento === "dinheiro" && !dinheiroPreenchido) {
            abrirPedidoModal("modalDinheiro");
            alert("Informe se precisa de troco para finalizar.");
            return;
        }
        if (pagamento === "dinheiro" && !validarDinheiro(totais.total)) {
            abrirPedidoModal("modalDinheiro");
            alert("Informe uma nota com valor suficiente para o total do carrinho.");
            return;
        }

        const pedido = montarComprovante(carrinho, totais);
        salvarPedido(pedido);
        document.getElementById("modalCarrinho")?.classList.remove("active");
        abrirComprovante(pedido.comprovante);
        carrinho = [];
        cartaoPreenchido = false;
        dinheiroPreenchido = false;
        renderizarCarrinho();
    }

    function salvarPedido(pedido) {
        const user = auth.getCurrentUser?.();
        if (!user) return;
        const enderecoAtual = document.getElementById("enderecoCliente").value.trim();
        auth.saveUserProfile({
            ...user,
            nome: document.getElementById("nomeCliente").value.trim() || user.nome,
            telefone: document.getElementById("telefoneCliente").value.trim() || user.telefone,
            endereco: user.endereco || enderecoAtual,
            pedidos: [pedido, ...(user.pedidos || [])].slice(0, 20)
        });
    }

    [saborUm, saborDois, tipoPizza, tamanhoPizza, quantidadeEntrada, quantidadeBebida].forEach(campo => {
        campo.addEventListener("change", atualizarResumo);
        campo.addEventListener("input", atualizarResumo);
    });

    [
        { select: entradaPedido, input: quantidadeEntrada },
        { select: bebidaPedido, input: quantidadeBebida }
    ].forEach(adicional => {
        adicional.select.addEventListener("change", () => {
            adicional.input.value = adicional.select.value === "" ? 0 : Math.max(1, quantidade(adicional.input));
            atualizarResumo();
        });
    });

    document.querySelectorAll("input[name='pagamento']").forEach(pagamento => {
        pagamento.addEventListener("change", () => {
            atualizarResumo();
            if (pagamento.value === "cartao") abrirPedidoModal("modalCartao");
            if (pagamento.value === "dinheiro") abrirPedidoModal("modalDinheiro");
        });
    });

    document.querySelectorAll("[data-close]").forEach(botao => {
        botao.addEventListener("click", () => fecharPedidoModal(botao.dataset.close));
    });

    document.querySelectorAll(".pedido-modal").forEach(modal => {
        modal.addEventListener("click", event => {
            if (event.target === modal) modal.classList.remove("active");
        });
    });

    document.getElementById("salvarCartao").addEventListener("click", () => {
        if (!validarCartao()) {
            alert("Preencha todos os dados do cartão.");
            return;
        }
        cartaoPreenchido = true;
        fecharPedidoModal("modalCartao");
    });

    document.querySelectorAll("input[name='precisaTroco']").forEach(opcao => {
        opcao.addEventListener("change", () => {
            const precisa = document.querySelector("input[name='precisaTroco']:checked").value === "sim";
            document.getElementById("valorNotaGrupo").style.display = precisa ? "block" : "none";
        });
    });

    document.getElementById("salvarDinheiro").addEventListener("click", () => {
        if (!validarDinheiro()) {
            alert("Informe uma nota com valor suficiente para calcular o troco.");
            return;
        }
        dinheiroPreenchido = true;
        fecharPedidoModal("modalDinheiro");
    });

    adicionarCarrinho.addEventListener("click", () => {
        const form = document.getElementById("pedidoForm");
        const resumo = obterResumo();

        if (!form.reportValidity()) return;
        carrinho.push(criarItemCarrinho(resumo));
        renderizarCarrinho();
        limparMontagemPizza();
    });

    atualizarResumo();
}

function abrirComprovante(comprovante) {
    let modal = document.getElementById("modalComprovante");
    if (!modal) {
        modal = document.createElement("div");
        modal.id = "modalComprovante";
        modal.className = "pedido-modal comprovante-modal";
        document.body.appendChild(modal);
        modal.addEventListener("click", event => {
            if (event.target === modal) modal.classList.remove("active");
        });
    }

    modal.innerHTML = `
        <div class="comprovante-box">
            <button class="comprovante-close" type="button" aria-label="Fechar comprovante">&times;</button>
            <div class="comprovante-status">
                <span class="comprovante-check">OK</span>
                <div>
                    <h2>Pedido confirmado</h2>
                    <p>Guarde este comprovante para acompanhar sua entrega.</p>
                </div>
            </div>
            <div class="comprovante-card">
                <div class="comprovante-topo">
                    <strong>Rocket Pizza</strong>
                    <span>Pedido #${comprovante.numero}</span>
                </div>
                <div class="comprovante-total">
                    <span>Total pago</span>
                    <strong>${comprovante.total}</strong>
                </div>
                <div class="comprovante-grid">
                    <span>Cliente</span><strong>${comprovante.nome}</strong>
                    <span>Telefone</span><strong>${comprovante.telefone}</strong>
                    <span>Endereço</span><strong>${comprovante.endereco}</strong>
                    <span>Pizza</span><strong>${comprovante.sabores}</strong>
                    <span>Entrada</span><strong>${comprovante.entrada}</strong>
                    <span>Bebida</span><strong>${comprovante.bebida}</strong>
                    <span>Entrega</span><strong>${comprovante.entrega}</strong>
                    <span>Desconto</span><strong>${comprovante.desconto}</strong>
                    <span>Pagamento</span><strong>${comprovante.pagamento}</strong>
                    ${comprovante.troco ? `<span>Troco</span><strong>${comprovante.troco}</strong>` : ""}
                </div>
                <p class="comprovante-observacao"><strong>Observações:</strong> ${comprovante.observacoes}</p>
            </div>
            <button class="finalizar-btn comprovante-ok" type="button">Fechar comprovante</button>
        </div>
    `;

    modal.querySelector(".comprovante-close").addEventListener("click", () => modal.classList.remove("active"));
    modal.querySelector(".comprovante-ok").addEventListener("click", () => modal.classList.remove("active"));
    modal.classList.add("active");
}

function popularAdicionais(select, itens, moeda) {
    select.add(new Option("Não incluir", ""));
    itens.forEach((item, index) => {
        select.add(new Option(`${item.nome} - ${moeda(item.preco)}`, index));
    });
}
