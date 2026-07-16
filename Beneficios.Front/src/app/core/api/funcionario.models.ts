export type TipoContrato = 'Clt' | 'Estagio' | 'Terceirizado' | 'Pj';
export type SituacaoFuncionario = 'Ativo' | 'Afastado' | 'Desligado';
export type JornadaTrabalho =
  | 'QuarentaQuatroHorasClt'
  | 'QuarentaHorasSegSex'
  | 'TrintaSeisHoras'
  | 'DozePorTrintaSeis'
  | 'SeisPorUm'
  | 'CincoPorUm'
  | 'CincoPorDois'
  | 'TempoParcial'
  | 'EspecialCategoria';

export interface FuncionarioBeneficio {
  id?: string;
  codigoBeneficio: string;
  ativo: boolean;
  dataInicio: string | null;
  dataFim: string | null;
  optIn: boolean;
}

export interface Funcionario {
  id: string;
  nome: string;
  cpf: string;
  matricula: string | null;
  dataAdmissao: string;
  dataDesligamento: string | null;
  cargo: string;
  salarioBase: number;
  tipoContrato: TipoContrato;
  centroCusto: string | null;
  resCep: string | null;
  resLogradouro: string | null;
  resNumero: string | null;
  resComplemento: string | null;
  resBairro: string | null;
  resCidade: string | null;
  resUf: string | null;
  trabNomeLocal: string | null;
  trabCep: string | null;
  trabLogradouro: string | null;
  trabNumero: string | null;
  trabComplemento: string | null;
  trabBairro: string | null;
  trabCidade: string | null;
  trabUf: string | null;
  situacao: SituacaoFuncionario;
  motivoAfastamentoAtivo: string | null;
  jornada: JornadaTrabalho;
  jornadaDetalhe: string | null;
  dataInclusao?: string;
  dataAlteracao?: string | null;
  beneficios: FuncionarioBeneficio[];
}

export interface FuncionarioFiltroRequest {
  nome?: string;
  cpf?: string;
  matricula?: string;
  situacao?: SituacaoFuncionario | '';
}

export interface FuncionarioSalvarRequest {
  nome: string;
  cpf: string;
  matricula: string | null;
  dataAdmissao: string;
  dataDesligamento: string | null;
  cargo: string;
  salarioBase: number;
  tipoContrato: TipoContrato;
  centroCusto: string | null;
  resCep: string | null;
  resLogradouro: string | null;
  resNumero: string | null;
  resComplemento: string | null;
  resBairro: string | null;
  resCidade: string | null;
  resUf: string | null;
  trabNomeLocal: string | null;
  trabCep: string | null;
  trabLogradouro: string | null;
  trabNumero: string | null;
  trabComplemento: string | null;
  trabBairro: string | null;
  trabCidade: string | null;
  trabUf: string | null;
  situacao: SituacaoFuncionario;
  jornada: JornadaTrabalho;
  jornadaDetalhe: string | null;
  beneficios: FuncionarioBeneficio[];
}

export const JORNADAS = [
  { valor: 'QuarentaQuatroHorasClt' as const, label: '44h semanais (CLT padrão)' },
  { valor: 'QuarentaHorasSegSex' as const, label: '40h semanais (segunda a sexta)' },
  { valor: 'TrintaSeisHoras' as const, label: '36h semanais (turnos reduzidos)' },
  { valor: 'DozePorTrintaSeis' as const, label: '12x36 (12h trabalho / 36h descanso)' },
  { valor: 'SeisPorUm' as const, label: '6x1 (trabalha 6 dias, folga 1)' },
  { valor: 'CincoPorUm' as const, label: '5x1 (trabalha 5 dias, folga 1)' },
  { valor: 'CincoPorDois' as const, label: '5x2 (trabalha 5 dias, folga 2)' },
  { valor: 'TempoParcial' as const, label: 'Tempo parcial (até 30h semanais)' },
  { valor: 'EspecialCategoria' as const, label: 'Jornada especial por categoria' },
] as const;

export const TIPOS_CONTRATO = [
  { valor: 'Clt' as const, label: 'CLT' },
  { valor: 'Estagio' as const, label: 'Estágio' },
  { valor: 'Terceirizado' as const, label: 'Terceirizado' },
  { valor: 'Pj' as const, label: 'PJ' },
] as const;

export const SITUACOES = [
  { valor: 'Ativo' as const, label: 'Ativo' },
  { valor: 'Afastado' as const, label: 'Afastado' },
  { valor: 'Desligado' as const, label: 'Desligado' },
] as const;

/** Situações editáveis no formulário (afastado é derivado). */
export const SITUACOES_EDITAVEIS = [
  { valor: 'Ativo' as const, label: 'Ativo' },
  { valor: 'Desligado' as const, label: 'Desligado' },
] as const;

/** Unidades federativas do Brasil (código IBGE / UF). */
export const UFS_BRASIL = [
  { valor: 'AC', label: 'AC — Acre' },
  { valor: 'AL', label: 'AL — Alagoas' },
  { valor: 'AP', label: 'AP — Amapá' },
  { valor: 'AM', label: 'AM — Amazonas' },
  { valor: 'BA', label: 'BA — Bahia' },
  { valor: 'CE', label: 'CE — Ceará' },
  { valor: 'DF', label: 'DF — Distrito Federal' },
  { valor: 'ES', label: 'ES — Espírito Santo' },
  { valor: 'GO', label: 'GO — Goiás' },
  { valor: 'MA', label: 'MA — Maranhão' },
  { valor: 'MT', label: 'MT — Mato Grosso' },
  { valor: 'MS', label: 'MS — Mato Grosso do Sul' },
  { valor: 'MG', label: 'MG — Minas Gerais' },
  { valor: 'PA', label: 'PA — Pará' },
  { valor: 'PB', label: 'PB — Paraíba' },
  { valor: 'PR', label: 'PR — Paraná' },
  { valor: 'PE', label: 'PE — Pernambuco' },
  { valor: 'PI', label: 'PI — Piauí' },
  { valor: 'RJ', label: 'RJ — Rio de Janeiro' },
  { valor: 'RN', label: 'RN — Rio Grande do Norte' },
  { valor: 'RS', label: 'RS — Rio Grande do Sul' },
  { valor: 'RO', label: 'RO — Rondônia' },
  { valor: 'RR', label: 'RR — Roraima' },
  { valor: 'SC', label: 'SC — Santa Catarina' },
  { valor: 'SP', label: 'SP — São Paulo' },
  { valor: 'SE', label: 'SE — Sergipe' },
  { valor: 'TO', label: 'TO — Tocantins' },
] as const;
